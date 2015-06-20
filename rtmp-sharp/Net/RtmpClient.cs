using Complete;
using Complete.Threading;
using RtmpSharp.IO;
using RtmpSharp.Messaging;
using RtmpSharp.Messaging.Events;
using RtmpSharp.Messaging.Messages;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RtmpSharp.Net
{
    public class RtmpClient
    {
        public event EventHandler Disconnected;
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        internal event EventHandler<RemotingMessageReceivedEventArgs> ServerMessageReceived;
        internal event EventHandler<CommandMessageReceivedEventArgs> ServerCommandReceived;
        public event EventHandler<Exception> CallbackException;

        public bool IsDisconnected { get { return !hasConnected || disconnectsFired != 0; } }
        
        public string ClientId;

        public bool NoDelay = true;
        public bool ExclusiveAddressUse;
        public int ReceiveTimeout;
        public int SendTimeout;
        public IPEndPoint LocalEndPoint;

        // by default, accept all certificates
        readonly Uri uri;
        readonly ObjectEncoding objectEncoding;
        readonly TaskCallbackManager<int, object> callbackManager;
        readonly SerializationContext serializationContext;
        readonly RemoteCertificateValidationCallback certificateValidator = (sender, certificate, chain, errors) => true;
        RtmpPacketWriter writer;
        RtmpPacketReader reader;
        Thread writerThread;
        Thread readerThread;

        int invokeId;
        bool hasConnected;
        bool throwDisconnect;

        volatile int disconnectsFired;

        public RtmpClient(Uri uri, SerializationContext serializationContext)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            if (serializationContext == null) throw new ArgumentNullException("serializationContext");

            var scheme = uri.Scheme.ToLowerInvariant();
            if (scheme != "rtmp" && scheme != "rtmps")
                throw new ArgumentException("Only rtmp:// and rtmps:// connections are supported.");

            this.uri = uri;
            this.serializationContext = serializationContext;
            callbackManager = new TaskCallbackManager<int, object>();
        }

        public RtmpClient(Uri uri, SerializationContext serializationContext, ObjectEncoding objectEncoding) : this(uri, serializationContext)
        {
            this.objectEncoding = objectEncoding;
        }

        public RtmpClient(Uri uri, ObjectEncoding objectEncoding, SerializationContext serializationContext, RemoteCertificateValidationCallback certificateValidator)
            : this(uri, serializationContext, objectEncoding)
        {
            if (certificateValidator == null) throw new ArgumentNullException("certificateValidator");

            this.certificateValidator = certificateValidator;
        }

        public RtmpClient(Uri uri, SerializationContext serializationContext, Stream connectedStream) : this(uri, serializationContext, ObjectEncoding.Amf3)
        {
            EstablishThreads(connectedStream);
            hasConnected = true;
            throwDisconnect = false;
        }






        void OnDisconnected(ExceptionalEventArgs e)
        {
            #pragma warning disable 420
            if (Interlocked.Increment(ref disconnectsFired) > 1)
                return;
            #pragma warning restore 420

            if (writer != null) writer.Continue = false;
            if (reader != null) reader.Continue = false;

            try { writerThread.Abort(); } catch { }
            try { readerThread.Abort(); } catch { }

            WrapCallback(() =>
            {
                if (Disconnected != null)
                    Disconnected(this, e);
            });

            if (throwDisconnect)
            {
                WrapCallback(() => callbackManager.SetExceptionForAll(
                    new ClientDisconnectedException(e.Description, e.Exception)));
            }
        }

        Task<object> QueueCommandAsTask(Command command, int streamId, int messageStreamId, bool requireConnected = true)
        {
            if (requireConnected && IsDisconnected)
                return CreateExceptedTask(new ClientDisconnectedException("disconnected"));

            var task = callbackManager.Create(command.InvokeId);
            writer.Queue(command, streamId, messageStreamId);
            return task;
        }

        public virtual async Task ConnectAsync()
        {
            if (hasConnected)
                return;

            var client = CreateTcpClient();
            client.NoDelay = NoDelay;
            client.ReceiveTimeout = ReceiveTimeout;
            client.SendTimeout = SendTimeout;
            client.ExclusiveAddressUse = ExclusiveAddressUse;

            await client.ConnectAsync(uri.Host, uri.Port);
            var stream = await GetRtmpStreamAsync(client);


            var random = new Random();
            var randomBytes = new byte[1528];
            random.NextBytes(randomBytes);

            // write c0+c1
            var c01 = new RtmpHandshake()
            {
                Version = 3,
                Time = (uint)Environment.TickCount,
                Time2 = 0,
                Random = randomBytes
            };
            await RtmpHandshake.WriteAsync(stream, c01, true);

            // read s0+s1
            var s01 = await RtmpHandshake.ReadAsync(stream, true);

            // write c2
            var c2 = s01.Clone();
            c2.Time2 = (uint)Environment.TickCount;
            await RtmpHandshake.WriteAsync(stream, c2, false);

            // read s2
            var s2 = await RtmpHandshake.ReadAsync(stream, false);

            // handshake check. won't work if running a local server (time will be the same) so run on debug
            #if !DEBUG
            if (!c01.Random.SequenceEqual(s2.Random) || c01.Time != s2.Time)
                throw new ProtocolViolationException();
            #endif

            EstablishThreads(stream);

            // call `connect`
            var connectResult = await ConnectInvokeAsync(null, null, uri.ToString());
            object cId;

            if (connectResult.TryGetValue("clientId", out cId))
                ClientId = cId as string;
            if (connectResult.TryGetValue("id", out cId))
                ClientId = cId as string;

            hasConnected = true;
        }
        
        public async Task RecreateConnection(string token) 
        {
            if (!hasConnected)
                return;

            var client = CreateTcpClient();
            client.NoDelay = NoDelay;
            client.ReceiveTimeout = ReceiveTimeout;
            client.SendTimeout = SendTimeout;
            client.ExclusiveAddressUse = ExclusiveAddressUse;

            await client.ConnectAsync(uri.Host, uri.Port);
            var stream = await GetRtmpStreamAsync(client);

            var random = new Random();
            var randomBytes = new byte[1528];
            random.NextBytes(randomBytes);

            // write c0+c1
            var c01 = new RtmpHandshake()
            {
                Version = 3,
                Time = (uint)Environment.TickCount,
                Time2 = 0,
                Random = randomBytes
            };
            await RtmpHandshake.WriteAsync(stream, c01, true);

            // read s0+s1
            var s01 = await RtmpHandshake.ReadAsync(stream, true);

            // write c2
            var c2 = s01.Clone();
            c2.Time2 = (uint)Environment.TickCount;
            await RtmpHandshake.WriteAsync(stream, c2, false);

            // read s2
            var s2 = await RtmpHandshake.ReadAsync(stream, false);

            EstablishThreads(stream);

#pragma warning disable 420
            Interlocked.Exchange(ref disconnectsFired, 0);
            Interlocked.Exchange(ref invokeId, 0);
#pragma warning restore 420

            var connectResult = await ReconnectInvokeAsync(null, null, uri.ToString(), token);
        }

        public void EstablishThreads(Stream stream)
        {
            writer = new RtmpPacketWriter(new AmfWriter(stream, serializationContext), ObjectEncoding.Amf3);
            reader = new RtmpPacketReader(new AmfReader(stream, serializationContext));
            reader.EventReceived += EventReceivedCallback;
            reader.Disconnected += OnPacketProcessorDisconnected;
            writer.Disconnected += OnPacketProcessorDisconnected;

            writerThread = new Thread(reader.ReadLoop) { IsBackground = true };
            readerThread = new Thread(writer.WriteLoop) { IsBackground = true };

            writerThread.Start();
            readerThread.Start();
        }

        public void Close()
        {
            OnDisconnected(new ExceptionalEventArgs("disconnected"));
        }

        TcpClient CreateTcpClient()
        {
            if (LocalEndPoint == null)
                return new TcpClient();
            return new TcpClient(LocalEndPoint);
        }

        async Task<Stream> GetRtmpStreamAsync(TcpClient client)
        {
            var stream = client.GetStream();
            switch (uri.Scheme)
            {
                case "rtmp":
                    return stream;
                case "rtmps":
                    try
                    {
                        //This causes a crash sometimes
                        var ssl = new SslStream(stream, false, certificateValidator);
                        await ssl.AuthenticateAsClientAsync(uri.Host);
                        return ssl;
                    }
                    catch
                    {
                        throw new ArgumentException("SSL error");
                    }
                default:
                    throw new ArgumentException("The specified scheme is not supported.");
            }
        }

        void OnPacketProcessorDisconnected(object sender, ExceptionalEventArgs args)
        {
            OnDisconnected(args);
        }

        async void EventReceivedCallback(object sender, EventReceivedEventArgs e)
        {
            if (e.Event == null)
                return;
            switch (e.Event.MessageType)
            {
                case MessageType.UserControlMessage:
                    var m = (UserControlMessage)e.Event;
                    if (m.EventType == UserControlMessageType.PingRequest)
                        WriteProtocolControlMessage(new UserControlMessage(UserControlMessageType.PingResponse, m.Values));
                    break;

                case MessageType.DataAmf3:
#if DEBUG
                    // Have no idea what the contents of these packets are.
                    // Study these packets if we receive them.
                    System.Diagnostics.Debugger.Break();
#endif
                    break;
                case MessageType.CommandAmf3:
                case MessageType.DataAmf0:
                case MessageType.CommandAmf0:
                    var command = (Command)e.Event;
                    var call = command.MethodCall;

                    var param = call.Parameters.Length == 1 ? call.Parameters[0] : call.Parameters;
                    if (call.Name == "_result")
                    {
                        // unwrap Flex class, if present
                        var ack = param as AcknowledgeMessageExt;
                        callbackManager.SetResult(command.InvokeId, ack != null ? ack.Body : param);
                    }
                    else if (call.Name == "_error")
                    {
                        // unwrap Flex class, if present
                        var error = param as ErrorMessage;
                        callbackManager.SetException(command.InvokeId, error != null ? new InvocationException(error) : new InvocationException());
                    }
                    else if (call.Name == "receive")
                    {
                        var message = param as AsyncMessageExt;
                        if (message == null)
                            break;

                        object subtopicObject;
                        message.Headers.TryGetValue(AsyncMessageHeaders.Subtopic, out subtopicObject);

                        var dsSubtopic = subtopicObject as string;
                        var clientId = message.ClientId;
                        var body = message.Body;

                        WrapCallback(() =>
                        {
                            if (MessageReceived != null)
                                MessageReceived(this, new MessageReceivedEventArgs(clientId, dsSubtopic, body));
                        });
                    }
                    else if (call.Name == "onstatus")
                    {
                        System.Diagnostics.Debug.Print("Received status.");
                    }
                    else if (call.Name == "connect")
                    {
                        await ConnectResultInvokeAsync(call.Parameters);
                    }
                    else if (param is RemotingMessage)
                    {
                        var message = param as RemotingMessage;

                        object endpoint;
                        message.Headers.TryGetValue(AsyncMessageHeaders.Endpoint, out endpoint);
                        object id;
                        message.Headers.TryGetValue(AsyncMessageHeaders.ID, out id);

                        WrapCallback(() =>
                        {
                            if (ServerMessageReceived != null)
                                ServerMessageReceived(this, new RemotingMessageReceivedEventArgs(message, message.Operation, endpoint as string, message.Destination, message.MessageId, message.Body,command.InvokeId));
                        });
                    }
                    else if (param is CommandMessage)
                    {
                        var message = param as CommandMessage;

                        object endpoint;
                        message.Headers.TryGetValue(AsyncMessageHeaders.Endpoint, out endpoint);
                        object id;
                        message.Headers.TryGetValue(AsyncMessageHeaders.ID, out id);

                        WrapCallback(() =>
                        {
                            if (ServerCommandReceived != null)
                                ServerCommandReceived(this, new CommandMessageReceivedEventArgs(message, endpoint as string, id as string, command.InvokeId));
                        });
                    }
                    else
                    {
#if DEBUG
                        System.Diagnostics.Debug.Print("Unknown RTMP Command: " + call.Name);
                        System.Diagnostics.Debugger.Break();
#endif
                    }
                    break;
            }
        }




        public Task<T> InvokeAsync<T>(string method, object argument)
        {
            return InvokeAsync<T>(method, new[] { argument });
        }

        public async Task<T> InvokeAsync<T>(string method, object[] arguments)
        {
            var invoke = new InvokeAmf0
            {
                MethodCall = new Method(method, arguments),
                InvokeId = GetNextInvokeId()
            };
            var result = await QueueCommandAsTask(invoke, 3, 0);
            return (T)MiniTypeConverter.ConvertTo(result, typeof(T));
        }

        public Task<T> InvokeAsync<T>(string endpoint, string destination, string method, object argument)
        {
            return InvokeAsync<T>(endpoint, destination, method, new[] { argument });
        }

        internal async Task<T> InvokeAsync<T>(RemotingMessage message)
        {

            var invoke = new InvokeAmf3()
            {
                InvokeId = GetNextInvokeId(),
                MethodCall = new Method(null, new object[] { message })
            };
            var result = await QueueCommandAsTask(invoke, 3, 0);
            return (T)MiniTypeConverter.ConvertTo(result, typeof(T));
        }

        public async Task<T> InvokeAsync<T>(string endpoint, string destination, string method, object[] arguments)
        {
            if (objectEncoding != ObjectEncoding.Amf3)
                throw new NotSupportedException("Flex RPC requires AMF3 encoding.");
            var remotingMessage = new RemotingMessage
            {
                ClientId = ClientId,//Guid.NewGuid().ToString("D"),
                Destination = destination,
                Operation = method,
                Body = arguments,
                Headers = new AsObject
                {
                    { FlexMessageHeaders.Endpoint, endpoint },
                    { FlexMessageHeaders.FlexClientId, ClientId ?? "nil" },
                    { FlexMessageHeaders.RequestTimeout, 60 }

                }
            };

            var invoke = new InvokeAmf3()
            {
                InvokeId = GetNextInvokeId(),
                MethodCall = new Method(null, new object[] { remotingMessage })
            };

            //HACK Return null if result is "_error"

            object result;
            try
            {
                result = await QueueCommandAsTask(invoke, 3, 0);
            }
            catch
            {
                result = null;
            }
            return (T)MiniTypeConverter.ConvertTo(result, typeof(T));
        }

        internal void InvokeResult(int invokeId, AcknowledgeMessageExt message)
        {
            if (objectEncoding != ObjectEncoding.Amf3)
                throw new NotSupportedException("Flex RPC requires AMF3 encoding.");

            var invoke = new InvokeAmf3()
            {
                InvokeId = invokeId,
                MethodCall = new Method("_result", new object[] { message }, true, CallStatus.Result)
            };
            QueueCommandAsTask(invoke, 3, 0);
        }

        public void InvokeResult(int invokeId, string correlationId, object argument)
        {
            var call = new AcknowledgeMessageExt()
            {
                ClientId = Uuid.NewUuid(),
                MessageId = Uuid.NewUuid(),
                Body = argument,
                CorrelationId = correlationId,
            };

            InvokeResult(invokeId, call);
        }

        public void InvokeReceive(string clientId, string subtopic, object body)
        {
            var invoke = new InvokeAmf3()
            {
                InvokeId = 0,
                MethodCall = new Method("receive", new object[] 
                { 
                    new AsyncMessageExt
                    {
                        Headers = new AsObject { { FlexMessageHeaders.FlexSubtopic, subtopic } },
                        ClientId = clientId,
                        Body = body,
                        MessageId = Uuid.NewUuid()
                    }
                })
            };
            QueueCommandAsTask(invoke, 3, 0);
        }

        public void InvokeDestReceive(string clientId, string subtopic, string destination, object body)
        {
            var invoke = new InvokeAmf3()
            {
                InvokeId = 0,
                MethodCall = new Method("receive", new object[] 
                { 
                    new AsyncMessageExt
                    {
                        Headers = new AsObject { { FlexMessageHeaders.FlexSubtopic, subtopic } },
                        ClientId = clientId,
                        Destination = destination,
                        Body = body,
                        MessageId = Uuid.NewUuid()
                    }
                })
            };
            QueueCommandAsTask(invoke, 3, 0);
        }

        public async Task<AsObject> ConnectResultInvokeAsync(object[] parameters)
        {
            //Write ServerBW & ClientBW
            var ServerBW = new WindowAcknowledgementSize(245248000);
            WriteProtocolControlMessage(ServerBW);
            var ClientBW = new PeerBandwidth(250000, 2);
            WriteProtocolControlMessage(ClientBW);

            SetChunkSize(50000);
            ClientId = Uuid.NewUuid();
            var connectResult = new InvokeAmf0
            {
                MethodCall = new Method("_result", new object[1] {
                    new AsObject
                               {
                                   { "objectEncoding", 3.0                    },
                                   { "level", "status"                        },
                                   { "details", null                          },
                                   { "description", "Connection succeeded."   },
                                   { "DSMessagingVersion", 1.0                },
                                   { "code", "NetConnection.Connect.Success"  },
                                   { "id", Uuid.NewUuid() }
                               }
                }),
                InvokeId = GetNextInvokeId()
            };

            return (AsObject)await QueueCommandAsTask(connectResult, 3, 0);
        }

        async Task<AsObject> ConnectInvokeAsync(string pageUrl, string swfUrl, string tcUrl)
        {
            var connect = new InvokeAmf0
            {
                MethodCall = new Method("connect", new object[] {false, "nil", "", new CommandMessage
                    { 
                        Operation = CommandOperation.ClientPing,
                        CorrelationId = "",
                        MessageId = Uuid.NewUuid(),
                        Destination = "",
                        Headers = new AsObject
                        {
                            { FlexMessageHeaders.FlexMessagingVersion, 1.0 },
                            { FlexMessageHeaders.FlexClientId, "my-rtmps" },
                        }
                    }
                }),
                ConnectionParameters =
                    new AsObject
                    {
                        { "pageUrl",           pageUrl                },
                        { "objectEncoding",    (double)objectEncoding },
                        { "capabilities",      239.0                  },
                        { "audioCodecs",       3575.0                 },
                        { "flashVer",          "WIN 16,0,0,228"       },
                        { "swfUrl",            "app:/LolClient.swf/[[DYNAMIC]]/47"},
                        { "videoFunction",     1.0                    },
                        { "fpad",              false                  },
                        { "videoCodecs",       252.0                  },
                        { "tcUrl",             tcUrl                  },
                        { "app",               ""                     }
                    },
                InvokeId = GetNextInvokeId()
                
            };

            return (AsObject)await QueueCommandAsTask(connect, 3, 0, requireConnected: false);
        }

        async Task<AsObject> ReconnectInvokeAsync(string pageUrl, string swfUrl, string tcUrl, string token)
        {
            var connect = new InvokeAmf0
            {
                MethodCall = new Method("connect", new object[] {false, ClientId, token, new CommandMessage
                    { 
                        Operation = CommandOperation.Login,
                        CorrelationId = "",
                        MessageId = Uuid.NewUuid(),
                        Destination = "",
                        Body = token,
                        Headers = new AsObject
                        {
                            { FlexMessageHeaders.FlexMessagingVersion, 1.0 },
                            { FlexMessageHeaders.FlexClientId, "my-rtmps" },
                        }
                    }
                }),
                ConnectionParameters =
                    new AsObject
                    {
                        { "pageUrl",           pageUrl                },
                        { "objectEncoding",    (double)objectEncoding },
                        { "capabilities",      239.0                  },
                        { "audioCodecs",       3575.0                 },
                        { "flashVer",          "WIN 16,0,0,228"       },
                        { "swfUrl",            "app:/LolClient.swf/[[DYNAMIC]]/47"},
                        { "videoFunction",     1.0                    },
                        { "fpad",              false                  },
                        { "videoCodecs",       252.0                  },
                        { "tcUrl",             tcUrl                  },
                        { "app",               ""                     }
                    },
                InvokeId = GetNextInvokeId()

            };

            return (AsObject)await QueueCommandAsTask(connect, 3, 0, requireConnected: false);
        }

        public async Task<bool> SubscribeAsync(string endpoint, string destination, string subtopic, string clientId)
        {
            var message = new CommandMessage
            {
                ClientId = clientId,
                CorrelationId = null,
                Operation = CommandOperation.Subscribe,
                Destination = destination,
                Headers = new AsObject
                {
                    { FlexMessageHeaders.Endpoint, endpoint },
                    { FlexMessageHeaders.FlexClientId, clientId },
                    { AsyncMessageHeaders.Subtopic, subtopic }
                }
            };
            return await InvokeAsync<string>(null, message) == "success";
        }

        public async Task<bool> UnsubscribeAsync(string endpoint, string destination, string subtopic, string clientId)
        {
            var message = new CommandMessage
            {
                ClientId = clientId,
                CorrelationId = null,
                Operation = CommandOperation.Unsubscribe,
                Destination = destination,
                Headers = new AsObject
                {
                    { FlexMessageHeaders.Endpoint, endpoint },
                    { FlexMessageHeaders.FlexClientId, clientId },
                    { AsyncMessageHeaders.Subtopic, subtopic }
                }
            };
            return await InvokeAsync<string>(null, message) == "success";
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var message = new CommandMessage
            {
                ClientId = ClientId,
                Destination = string.Empty, // destination must not be null to work on some servers
                Operation = CommandOperation.Login,
                Body = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", username, password))),
            };
            return await InvokeAsync<string>(null, message) == "success";
        }

        public async Task<bool> LoginAsync(string base64)
        {
            var message = new CommandMessage
            {
                ClientId = ClientId,
                Destination = string.Empty, // destination must not be null to work on some servers
                Operation = CommandOperation.Login,
                Body = base64,
            };
            return await InvokeAsync<string>(null, message) == "success";
        }

        public Task LogoutAsync()
        {
            var message = new CommandMessage
            {
                ClientId = ClientId,
                Destination = string.Empty,
                Operation = CommandOperation.Logout
            };
            return InvokeAsync<object>(null, message);
        }

        public void SetChunkSize(int size)
        {
            WriteProtocolControlMessage(new ChunkSize(size));
        }

        public Task PingAsync()
        {
            var message = new CommandMessage
            {
                ClientId = ClientId,
                Destination = string.Empty,
                Operation = CommandOperation.ClientPing
            };
            return InvokeAsync<object>(null, message);
        }

        void WriteProtocolControlMessage(RtmpEvent @event)
        {
            writer.Queue(@event, 2, 0);
        }

        int GetNextInvokeId()
        {
            // interlocked.increment wraps overflows
            return Interlocked.Increment(ref invokeId);
        }

        #pragma warning disable 0168 //Disable unused variable warning
        void WrapCallback(Action action)
        {
            try
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    if (CallbackException != null)
                        CallbackException(this, ex);
                }
            }
            catch (Exception unhandled)
            {
#if DEBUG && BREAK_ON_UNHANDLED_CALLBACK
                System.Diagnostics.Debug.Print("UNHANDLED EXCEPTION IN CALLBACK: {0}: {1} @ {2}", unhandled.GetType(), unhandled.Message, unhandled.StackTrace);
                System.Diagnostics.Debugger.Break();
#endif
            }
        }

        static Task<object> CreateExceptedTask(Exception exception)
        {
            var source = new TaskCompletionSource<object>();
            source.SetException(exception);
            return source.Task;
        }
    }
}