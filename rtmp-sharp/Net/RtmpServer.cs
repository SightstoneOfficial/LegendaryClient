using RtmpSharp.IO;
using RtmpSharp.Messaging;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using RtmpSharp.Messaging.Messages;

namespace RtmpSharp.Net
{
    public class RtmpServer
    {
        //TODO: Handle disconnecting
        public event EventHandler<RemotingMessageReceivedEventArgs> ClientMessageReceived;
        public event EventHandler<CommandMessageReceivedEventArgs> ClientCommandReceieved;

        private TcpListener _listener;
        private IPEndPoint _serverEndPoint;
        private Uri _serverUri;
        private SerializationContext _context;

        private List<RtmpClient> _clients;

        private readonly RemoteCertificateValidationCallback certificateValidator = (sender, certificate, chain, errors) => true;
        private X509Certificate2 Certificate;
        private bool SSL = false;

        public RtmpServer(IPEndPoint ServerEndPoint, SerializationContext context)
        {
            _serverEndPoint = ServerEndPoint;
            _listener = new TcpListener(_serverEndPoint);
            _context = context;

            if (_context == null)
                _context = new SerializationContext();

            _clients = new List<RtmpClient>();

            string ServerUri = string.Format("rtmp://{0}:{1}", _serverEndPoint.Address, _serverEndPoint.Port);
            _serverUri = new Uri(ServerUri);
        }

        public RtmpServer(IPEndPoint ServerEndPoint, SerializationContext context, X509Certificate2 certificate)
            : this(ServerEndPoint, context)
        {
            SSL = true;
            Certificate = certificate;

            string ServerUri = string.Format("rtmps://{0}:{1}", _serverEndPoint.Address, _serverEndPoint.Port);
            _serverUri = new Uri(ServerUri);
        }

        async void OnClientAccepted(IAsyncResult ar)
        {
            TcpListener listener = ar.AsyncState as TcpListener;
            if (listener == null)
                return;

            try
            {
                TcpClient client = listener.EndAcceptTcpClient(ar);
                var stream = await GetRtmpStreamAsync(client);

                // read c0+c1
                var c01 = await RtmpHandshake.ReadAsync(stream, true);

                var random = new Random();
                var randomBytes = new byte[1528];
                random.NextBytes(randomBytes);

                // write s0+s1+s2
                var s01 = new RtmpHandshake()
                {
                    Version = 3,
                    Time = (uint)Environment.TickCount,
                    Time2 = 0,
                    Random = randomBytes
                };
                var s02 = s01.Clone();
                s02.Time2 = (uint)Environment.TickCount;
                await RtmpHandshake.WriteAsync(stream, s01, s02, true);

                // read c02
                var c02 = await RtmpHandshake.ReadAsync(stream, false);

                RtmpClient rtmpClient = new RtmpClient(_serverUri, _context, stream);
                rtmpClient.ServerMessageReceived += ServerMessageReceived;
                rtmpClient.ServerCommandReceived += ServerCommandReceived;
                _clients.Add(rtmpClient);
            }
            finally
            {
                listener.BeginAcceptTcpClient(OnClientAccepted, listener);
            }
        }

        void ServerCommandReceived(object sender, CommandMessageReceivedEventArgs e)
        {
            RtmpClient client = (RtmpClient)sender;
            switch (e.Message.Operation)
            {
                case CommandOperation.Login:
                    AcknowledgeMessageExt login = new AcknowledgeMessageExt
                    {
                        Body = "success",
                        CorrelationId = e.Message.MessageId,
                        MessageId = Uuid.NewUuid(),
                        ClientId = Uuid.NewUuid(),
                        Headers = new AsObject
                        {
                            { "DSMessagingVersion", 1.0 },
                            { FlexMessageHeaders.FlexClientId, e.DSId }
                        }
                    };

                    client.InvokeResult(e.InvokeId, login);
                    break;
                case CommandOperation.Subscribe:
                    AcknowledgeMessageExt subscribe = new AcknowledgeMessageExt
                    {
                        Body = null,
                        CorrelationId = e.Message.MessageId,
                        MessageId = Uuid.NewUuid(),
                        ClientId = e.Message.ClientId
                    };

                    client.InvokeResult(e.InvokeId, subscribe);
                    break;
                case CommandOperation.Unsubscribe:
                    break;
                case CommandOperation.ClientPing:
                    break;
                case CommandOperation.Logout:
                    break;
                default:
                    throw new NotSupportedException();
            }

            if (ClientCommandReceieved != null)
            {
                ClientCommandReceieved(sender, e);
            }
        }

        void ServerMessageReceived(object sender, RemotingMessageReceivedEventArgs e)
        {
            if (ClientMessageReceived != null)
            {
                ClientMessageReceived(sender, e);
                if (e.ReturnRequired)
                {
                    RtmpClient client = (RtmpClient)sender;
                    client.InvokeResult(e.InvokeId, e.MessageId, e.Data);
                }
            }
        }



        public void Listen()
        {
            _listener.Start();
            _listener.BeginAcceptTcpClient(OnClientAccepted, _listener);
        }

        async Task<Stream> GetRtmpStreamAsync(TcpClient client)
        {
            var stream = client.GetStream();

            if (SSL && Certificate != null)
            {
                var ssl = new SslStream(stream, false, certificateValidator);
                await ssl.AuthenticateAsServerAsync(Certificate);
                return ssl;
            }
            else
            {
                return stream;
            }
        }
    }
}