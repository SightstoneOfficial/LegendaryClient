/**
 * A very basic RTMPS client
 *
 * @author Gabriel Van Eyck
 */
/////////////////////////////////////////////////////////////////////////////////
//
//Ported to C# by Ryan A. LaSarre
//
/////////////////////////////////////////////////////////////////////////////////

using PVPNetConnect.RiotObjects;
using PVPNetConnect.RiotObjects.Platform.Broadcast;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Game.Message;
using PVPNetConnect.RiotObjects.Platform.Matchmaking;
using PVPNetConnect.RiotObjects.Platform.Messaging;
using PVPNetConnect.RiotObjects.Platform.Statistics;
using PVPNetConnect.RiotObjects.Platform.Trade;
using PVPNetConnect.RiotObjects.Platform.Gameinvite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using PVPNetConnect.RiotObjects.Gameinvite.Contract;
using PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract;
using PVPNetConnect.RiotObjects.Platform.ServiceProxy.Dispatch;

namespace PVPNetConnect
{
    public partial class PVPNetConnection
    {
        public bool KeepDelegatesOnLogout = true;

        #region Member Declarations

        //RTMPS Connection Info
        private bool isConnected = false;

        private bool isLoggedIn = false;
        private TcpClient client;
        private SslStream sslStream;
        private string ipAddress;
        private string authToken;
        private int accountID;
        private string sessionToken;
        private string DSId;

        //Initial Login Information
        private string user;

        private string password;
        private string server;
        private string loginQueue;
        private string locale;
        private string clientVersion;

        /** Garena information */
        private bool useGarena = false;
        private string garenaToken;
        public string userID;

        //Invoke Variables
        private Random rand = new Random();

        private JavaScriptSerializer serializer = new JavaScriptSerializer();

        private int invokeID = 2;

        private List<int> pendingInvokes = new List<int>();
        private Dictionary<int, TypedObject> results = new Dictionary<int, TypedObject>();
        private Dictionary<int, RiotGamesObject> callbacks = new Dictionary<int, RiotGamesObject>();
        private Thread decodeThread;

        private int heartbeatCount = 1;
        private Thread heartbeatThread;

        #endregion Member Declarations

        #region Event Handlers

        public delegate void OnConnectHandler(object sender, EventArgs e);

        public event OnConnectHandler OnConnect;

        public delegate void OnLoginQueueUpdateHandler(object sender, int positionInLine);

        public event OnLoginQueueUpdateHandler OnLoginQueueUpdate;

        public delegate void OnLoginHandler(object sender, string username, string ipAddress);

        public event OnLoginHandler OnLogin;

        public delegate void OnDisconnectHandler(object sender, EventArgs e);

        public event OnDisconnectHandler OnDisconnect;

        public delegate void OnMessageReceivedHandler(object sender, object message);

        public event OnMessageReceivedHandler OnMessageReceived;

        public delegate void OnErrorHandler(object sender, Error error);

        public event OnErrorHandler OnError;

        #endregion Event Handlers

        #region Connect, Login, and Heartbeat Methods

        public void Connect(string user, string password, Region region, string clientVersion, bool cs = false, string Server = null, string Loginqueue = null, string Locales = null)
        {
            if (!isConnected)
            {
                Thread t = new Thread(() =>
                {
                    this.user = user;
                    this.password = password;
                    this.clientVersion = clientVersion;
                    //this.server = "127.0.0.1";
                    this.server = RegionInfo.GetServerValue(region);
                    this.loginQueue = RegionInfo.GetLoginQueueValue(region);
                    this.locale = RegionInfo.GetLocaleValue(region);
                    this.useGarena = RegionInfo.GetUseGarenaValue(region);

                    if (cs == true && Server != null && Loginqueue != null && Locales != null)
                    {
                        this.server = Server;
                        this.loginQueue = Loginqueue;
                        this.locale = Locales;
                        this.useGarena = false;
                    }


                    //Sets up our sslStream to riots servers
                    try
                    {
                        client = new TcpClient(server, 2099);
                    }
                    catch
                    {
                        Error("Riots servers are currently unavailable.", ErrorType.AuthKey);
                        Disconnect();
                        return;
                    }

                    //Check for riot webserver status
                    //along with gettin out Auth Key that we need for the login process.
                    if (useGarena)
                        if (!GetGarenaToken())
                            return;


                    if (!GetAuthKey())
                        return;

                    if (!GetIpAddress())
                        return;

                    sslStream = new SslStream(client.GetStream(), false, AcceptAllCertificates);
                    var ar = sslStream.BeginAuthenticateAsClient(server, null, null);
                    using (ar.AsyncWaitHandle)
                    {
                        if (ar.AsyncWaitHandle.WaitOne(-1))
                        {
                            sslStream.EndAuthenticateAsClient(ar);
                        }
                    }

                    if (!Handshake())
                        return;

                    BeginReceive();

                    if (!SendConnect())
                        return;

                    if (!Login())
                        return;

                    StartHeartbeat();
                });
                t.IsBackground = true;
                t.Start();
            }
        }

        private bool AcceptAllCertificates(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private bool GetGarenaToken()
        {            
            try
            {
                System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                byte[] junk = new byte[] { 0x49, 0x00, 0x00, 0x00, 0x10, 0x01, 0x00, 0x79, 0x2f };
                System.Security.Cryptography.MD5 md5Cryp = System.Security.Cryptography.MD5.Create();
                byte[] inputBytes = encoding.GetBytes(password);
                byte[] md5 = md5Cryp.ComputeHash(inputBytes);
                /*
                //GET OUR USER ID
                List<byte> userIdRequestBytes = new List<byte>();

                byte[] junk = new byte[] { 0x49, 0x00, 0x00, 0x00, 0x10, 0x01, 0x00, 0x79, 0x2f };
                userIdRequestBytes.AddRange(junk);
                userIdRequestBytes.AddRange(encoding.GetBytes(user));
                for (int i = 0; i < 16; i++)
                    userIdRequestBytes.Add(0x00);

                System.Security.Cryptography.MD5 md5Cryp = System.Security.Cryptography.MD5.Create();
                byte[] inputBytes = encoding.GetBytes(password);
                byte[] md5 = md5Cryp.ComputeHash(inputBytes);

                foreach (byte b in md5)
                    userIdRequestBytes.AddRange(encoding.GetBytes(String.Format("%02x", b)));

                userIdRequestBytes.Add(0x00);
                userIdRequestBytes.Add(0x01);
                junk = new byte[] { 0xD4, 0xAE, 0x52, 0xC0, 0x2E, 0xBA, 0x72, 0x03 };
                userIdRequestBytes.AddRange(junk);

                int timestamp = (int)(DateTime.UtcNow.TimeOfDay.TotalMilliseconds / 1000);
                for (int i = 0; i < 4; i++)
                    userIdRequestBytes.Add((byte)((timestamp >> (8 * i)) & 0xFF));

                userIdRequestBytes.Add(0x00);
                userIdRequestBytes.AddRange(encoding.GetBytes("intl"));
                userIdRequestBytes.Add(0x00);

                byte[] userIdBytes = userIdRequestBytes.ToArray();

                var client = new TcpClient("203.117.158.170", 9100);

                client.GetStream().Write(userIdBytes, 0, userIdBytes.Length);
                client.GetStream().Flush();

                int id = 0;
                for (int i = 0; i < 4; i++)
                    id += client.GetStream().ReadByte() * (1 << (8 * i));                
                userID = Convert.ToString(id);
                //*/
                //GET TOKEN
                List<byte> tokenRequestBytes = new List<byte>();
                junk = new byte[] { 0x32, 0x00, 0x00, 0x00, 0x01, 0x03, 0x80, 0x00, 0x00 };
                tokenRequestBytes.AddRange(junk);
                tokenRequestBytes.AddRange(encoding.GetBytes(user));
                tokenRequestBytes.Add(0x00);
                foreach (byte b in md5)
                    tokenRequestBytes.AddRange(encoding.GetBytes(String.Format("%02x", b)));
                tokenRequestBytes.Add(0x00);
                tokenRequestBytes.Add(0x00);
                tokenRequestBytes.Add(0x00);

                byte[] tokenBytes = tokenRequestBytes.ToArray();

                client = new TcpClient("lol.auth.garenanow.com", 12000);
                NetworkStream stream = client.GetStream();
                stream.Write(tokenBytes, 0, tokenBytes.Length);
                stream.Flush();
                byte[] bytes = new byte[client.ReceiveBufferSize];
                var x = stream.Read(bytes, 0, (int)client.ReceiveBufferSize);
                garenaToken = x.ToString();
                /*
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < 5; i++)
                    client.GetStream().ReadByte();
                int c;
                while ((c = client.GetStream().ReadByte()) != 0)
                    sb.Append((char)c);

                garenaToken = sb.ToString();

                client.GetStream().Flush();
                client.Close();
                //*/
                return true;
            }
            catch
            {
                //Error("Unable to acquire garena token", ErrorType.Login);
                Error("Garena is in dev. This may not work", ErrorType.Login);
                Disconnect();
                return false;
            }
        }

        void GetToken()
        {
            try
            {
                List<byte> tokenRequestBytes = new List<byte>();
                System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

                System.Security.Cryptography.MD5 md5Cryp = System.Security.Cryptography.MD5.Create();
                byte[] inputBytes = encoding.GetBytes(password);
                byte[] md5 = md5Cryp.ComputeHash(inputBytes);
                byte[] junk = new byte[] { 0x32, 0x00, 0x00, 0x00, 0x01, 0x03, 0x80, 0x00, 0x00 };
                tokenRequestBytes.AddRange(junk);
                tokenRequestBytes.AddRange(encoding.GetBytes(user));
                tokenRequestBytes.Add(0x00);
                foreach (byte b in md5)
                    tokenRequestBytes.AddRange(encoding.GetBytes(String.Format("%02x", b)));
                tokenRequestBytes.Add(0x00);
                tokenRequestBytes.Add(0x00);
                tokenRequestBytes.Add(0x00);

                byte[] tokenBytes = tokenRequestBytes.ToArray();

                client = new TcpClient("lol.auth.garenanow.com", 12000);
                client.GetStream().Write(tokenBytes, 0, tokenBytes.Length);
                //client.GetStream().Flush();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < 5; i++)
                    client.GetStream().ReadByte();
                int c;
                while ((c = client.GetStream().ReadByte()) != 0)
                    sb.Append((char)c);

                garenaToken = sb.ToString();
                client.GetStream().Flush();
                client.Close();

            }
            catch
            {

            }
        }

        private bool GetAuthKey()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                string payload = "user=" + user + ",password=" + password;
                string query = "payload=" + payload;

                if (useGarena)
                    payload = garenaToken;

                WebRequest con = WebRequest.Create(loginQueue + "login-queue/rest/queue/authenticate");
                con.Method = "POST";

                Stream outputStream = con.GetRequestStream();
                outputStream.Write(Encoding.ASCII.GetBytes(query), 0, Encoding.ASCII.GetByteCount(query));

                WebResponse webresponse = con.GetResponse();
                Stream inputStream = webresponse.GetResponseStream();

                int c;
                while ((c = inputStream.ReadByte()) != -1)
                    sb.Append((char)c);

                TypedObject result = serializer.Deserialize<TypedObject>(sb.ToString());
                outputStream.Close();
                inputStream.Close();
                con.Abort();

                if (!result.ContainsKey("token"))
                {
                    int node = (int)result.GetInt("node");
                    string champ = result.GetString("champ");
                    int rate = (int)result.GetInt("rate");
                    int delay = (int)result.GetInt("delay");

                    int id = 0;
                    int cur = 0;

                    object[] tickers = result.GetArray("tickers");
                    foreach (object o in tickers)
                    {
                        Dictionary<string, object> to = (Dictionary<string, object>)o;

                        int tnode = (int)to["node"];
                        if (tnode != node)
                            continue;

                        id = (int)to["id"];
                        cur = (int)to["current"];
                        break;
                    }

                    while (id - cur > rate)
                    {
                        sb.Clear();
                        if (OnLoginQueueUpdate != null)
                            OnLoginQueueUpdate(this, id - cur);

                        Thread.Sleep(delay);
                        con = WebRequest.Create(loginQueue + "login-queue/rest/queue/ticker/" + champ);
                        con.Method = "GET";
                        webresponse = con.GetResponse();
                        inputStream = webresponse.GetResponseStream();

                        int d;
                        while ((d = inputStream.ReadByte()) != -1)
                            sb.Append((char)d);

                        result = serializer.Deserialize<TypedObject>(sb.ToString());

                        inputStream.Close();
                        con.Abort();

                        if (result == null)
                            continue;

                        cur = HexToInt(result.GetString(node.ToString()));
                    }

                    while (sb.ToString() == null || !result.ContainsKey("token"))
                    {
                        try
                        {
                            sb.Clear();

                            if (id - cur < 0)
                                if (OnLoginQueueUpdate != null)
                                    OnLoginQueueUpdate(this, 0);
                                else if (OnLoginQueueUpdate != null)
                                    OnLoginQueueUpdate(this, id - cur);

                            Thread.Sleep(delay / 10);
                            con = WebRequest.Create(loginQueue + "login-queue/rest/queue/authToken/" + user.ToLower());
                            con.Method = "GET";
                            webresponse = con.GetResponse();
                            inputStream = webresponse.GetResponseStream();

                            int f;
                            while ((f = inputStream.ReadByte()) != -1)
                                sb.Append((char)f);

                            result = serializer.Deserialize<TypedObject>(sb.ToString());

                            inputStream.Close();
                            con.Abort();
                        }
                        catch
                        {
                        }
                    }
                }
                if (OnLoginQueueUpdate != null)
                    OnLoginQueueUpdate(this, 0);
                authToken = result.GetString("token");

                return true;
            }
            catch (Exception e)
            {
                if (e.Message == "The remote name could not be resolved: '" + loginQueue + "'")
                {
                    Error("Please make sure you are connected the internet!", ErrorType.AuthKey);
                    Disconnect();
                }
                else if (e.Message == "The remote server returned an error: (403) Forbidden.")
                {
                    Error("Your username or password is incorrect!", ErrorType.Password);
                    Disconnect();
                }
                else if (e.Message == "The given key was not present in the dictionary.")
                {
                    Error("The given key was not present in the dictionary. Client version is wrong maybe?", ErrorType.AuthKey);
                    Disconnect();
                }
                else
                {
                    Error("Unable to get Auth Key \n" + e, ErrorType.AuthKey);
                    Disconnect();
                }

                return false;
            }
        }

        private int HexToInt(string hex)
        {
            int total = 0;
            for (int i = 0; i < hex.Length; i++)
            {
                char c = hex.ToCharArray()[i];
                if (c >= '0' && c <= '9')
                    total = total * 16 + c - '0';
                else
                    total = total * 16 + c - 'a' + 10;
            }

            return total;
        }

        private bool GetIpAddress()
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                WebRequest con = WebRequest.Create("http://ll.leagueoflegends.com/services/connection_info");
                WebResponse response = con.GetResponse();

                int c;
                while ((c = response.GetResponseStream().ReadByte()) != -1)
                    sb.Append((char)c);

                con.Abort();

                TypedObject result = serializer.Deserialize<TypedObject>(sb.ToString());

                ipAddress = result.GetString("ip_address");

                return true;
            }
            catch (Exception e)
            {
                Error("Unable to connect to Riot Games web server \n" + e.Message, ErrorType.General);
                Disconnect();
                return false;
            }
        }

        public static string GetNewIpAddress()
        {
            StringBuilder sb = new StringBuilder();

            WebRequest con = WebRequest.Create("http://ll.leagueoflegends.com/services/connection_info");
            WebResponse response = con.GetResponse();

            int c;
            while ((c = response.GetResponseStream().ReadByte()) != -1)
                sb.Append((char)c);

            con.Abort();

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, string> deserializedJSON = serializer.Deserialize<Dictionary<string, string>>(sb.ToString());

            return deserializedJSON["ip_address"];
        }


        private bool Handshake()
        {
            byte[] handshakePacket = new byte[1537];
            rand.NextBytes(handshakePacket);
            handshakePacket[0] = (byte)0x03;
            sslStream.Write(handshakePacket);

            byte S0 = (byte)sslStream.ReadByte();
            if (S0 != 0x03)
            {
                Error("Server returned incorrect version in handshake: " + S0, ErrorType.Handshake);
                Disconnect();
                return false;
            }

            byte[] responsePacket = new byte[1536];
            sslStream.Read(responsePacket, 0, 1536);
            sslStream.Write(responsePacket);

            // Wait for response and discard result
            byte[] S2 = new byte[1536];
            sslStream.Read(S2, 0, 1536);

            // Validate handshake
            bool valid = true;
            for (int i = 8; i < 1536; i++)
            {
                if (handshakePacket[i + 1] != S2[i])
                {
                    valid = false;
                    break;
                }
            }

            if (!valid)
            {
                Error("Server returned invalid handshake", ErrorType.Handshake);
                Disconnect();
                return false;
            }
            return true;
        }

        private bool SendConnect()
        {
            Dictionary<string, object> paramaters = new Dictionary<string, object>();
            paramaters.Add("app", "");
            paramaters.Add("flashVer", "WIN 10,6,602,161");
            paramaters.Add("swfUrl", "app:/LolClient.swf/[[DYNAMIC]]/32");
            paramaters.Add("tcUrl", "rtmps://" + server + ":" + 2099);
            paramaters.Add("fpad", false);
            paramaters.Add("capabilities", 239);
            paramaters.Add("audioCodecs", 3575);
            paramaters.Add("videoCodecs", 252);
            paramaters.Add("videoFunction", 1);
            paramaters.Add("pageUrl", null);
            paramaters.Add("objectEncoding", 3);

            RTMPSEncoder encoder = new RTMPSEncoder();
            byte[] connect = encoder.EncodeConnect(paramaters);

            sslStream.Write(connect, 0, connect.Length);

            while (!results.ContainsKey(1))
                Thread.Sleep(10);
            TypedObject result = results[1];
            results.Remove(1);
            if (result["result"].Equals("_error"))
            {
                Error(GetErrorMessage(result), ErrorType.Connect);
                Disconnect();
                return false;
            }

            DSId = result.GetTO("data").GetString("id");

            isConnected = true;
            if (OnConnect != null)
                OnConnect(this, EventArgs.Empty);

            return true;
        }

        private bool Login()
        {
            TypedObject result, body;

            // Login 1
            body = new TypedObject("com.riotgames.platform.login.AuthenticationCredentials");
            body.Add("password", password);
            body.Add("clientVersion", clientVersion);
            body.Add("ipAddress", ipAddress);
            body.Add("securityAnswer", null);
            body.Add("locale", locale);
            body.Add("domain", "lolclient.lol.riotgames.com");

            body.Add("oldPassword", null);
            body.Add("authToken", authToken);
            if (useGarena)
            {
                body.Add("partnerCredentials", "8393 " + garenaToken);
                body.Add("username", userID);
            }
            else
            {
                body.Add("partnerCredentials", null);
                body.Add("username", user);
            }

            int id = Invoke("loginService", "login", new object[] { body });

            result = GetResult(id);
            if (result["result"].Equals("_error"))
            {
                Error(GetErrorMessage(result), ErrorType.Login);
                Disconnect();
                return false;
            }

            body = result.GetTO("data").GetTO("body");
            sessionToken = body.GetString("token");
            accountID = (int)body.GetTO("accountSummary").GetInt("accountId");

            // Login 2

            if (useGarena)
                body = WrapBody(Convert.ToBase64String(Encoding.UTF8.GetBytes(userID + ":" + sessionToken)), "auth", 8);
            else
                body = WrapBody(Convert.ToBase64String(Encoding.UTF8.GetBytes(user.ToLower() + ":" + sessionToken)),
                    "auth", 8);

            body.type = "flex.messaging.messages.CommandMessage";

            id = Invoke(body);
            result = GetResult(id); // Read result (and discard)

            isLoggedIn = true;
            if (OnLogin != null)
                OnLogin(this, user, ipAddress);
            return true;
        }

        private string GetErrorMessage(TypedObject message)
        {
            // Works for clientVersion
            return message.GetTO("data").GetTO("rootCause").GetString("message");
        }

        private string GetErrorCode(TypedObject message)
        {
            return message.GetTO("data").GetTO("rootCause").GetString("errorCode");
        }

        private void StartHeartbeat()
        {
            heartbeatThread = new Thread(async () =>
            {
                while (true)
                {
                    try
                    {
                        long hbTime = (long)DateTime.Now.TimeOfDay.TotalMilliseconds;
                        string result =
                            await
                                PerformLCDSHeartBeat(accountID, sessionToken, heartbeatCount,
                                    DateTime.Now.ToString("ddd MMM d yyyy HH:mm:ss 'GMT-0700'"));
                        //int id = Invoke("loginService", "performLCDSHeartBeat", new object[] { accountID, sessionToken, heartbeatCount, DateTime.Now.ToString("ddd MMM d yyyy HH:mm:ss 'GMT-0700'") });
                        //Cancel(id); // Ignore result for now

                        heartbeatCount++;

                        // Quick sleeps to shutdown the heartbeat quickly on a reconnect
                        while ((long)DateTime.Now.TimeOfDay.TotalMilliseconds - hbTime < 120000)
                            Thread.Sleep(100);
                    }
                    catch
                    {
                    }
                }
            });
            heartbeatThread.IsBackground = true;
            heartbeatThread.Start();
        }

        #endregion Connect, Login, and Heartbeat Methods

        #region Disconnect Methods

        public void Disconnect()
        {
            Thread t = new Thread(() =>
            {
                if (isConnected)
                {
                    int id = Invoke("loginService", "logout", new object[] { authToken });
                    Join(id);
                }

                isConnected = false;

                if (heartbeatThread != null)
                    heartbeatThread.Abort();

                if (decodeThread != null)
                    decodeThread.Abort();

                invokeID = 2;
                heartbeatCount = 1;
                pendingInvokes.Clear();
                callbacks.Clear();
                results.Clear();

                if (!KeepDelegatesOnLogout)
                {
                    try
                    {
                        foreach (Delegate d in OnMessageReceived.GetInvocationList())
                        {
                            OnMessageReceived -= (OnMessageReceivedHandler)d;
                        }
                    }
                    catch
                    {

                    }
                }

                client = null;
                sslStream = null;

                if (OnDisconnect != null)
                    OnDisconnect(this, EventArgs.Empty);
            });
            t.IsBackground = true;
            t.Start();
        }

        #endregion Disconnect Methods

        #region Error Methods

        private void Error(string message, string errorCode, ErrorType type)
        {
            Error error = new Error()
            {
                Type = type,
                Message = message,
                ErrorCode = errorCode
            };

            if (OnError != null)
                OnError(this, error);
        }

        private void Error(string message, ErrorType type)
        {
            Error(message, "", type);
        }

        #endregion Error Methods

        #region Send Methods

        private int Invoke(TypedObject packet)
        {
            int id = NextInvokeID();
            pendingInvokes.Add(id);

            try
            {
                RTMPSEncoder encoder = new RTMPSEncoder();
                byte[] data = encoder.EncodeInvoke(id, packet);

                sslStream.Write(data, 0, data.Length);

                return id;
            }
            catch (IOException e)
            {
                // Clear the pending invoke
                pendingInvokes.Remove(id);

                // Rethrow
                throw e;
            }
        }

        private int Invoke(string destination, object operation, object body)
        {
            return Invoke(WrapBody(body, destination, operation));
        }

        private int InvokeWithCallback(string destination, object operation, object body, RiotGamesObject cb)
        {
            if (isConnected)
            {
                callbacks.Add(invokeID, cb); // Register the callback
                return Invoke(destination, operation, body);
            }
            else
            {
                Error(
                    "The client is not connected. Please make sure to connect before tring to execute an Invoke command.",
                    ErrorType.Invoke);
                Disconnect();
                return -1;
            }
        }

        protected TypedObject WrapBody(object body, string destination, object operation)
        {
            TypedObject headers = new TypedObject();
            headers.Add("DSRequestTimeout", 60);
            headers.Add("DSId", DSId);
            headers.Add("DSEndpoint", "my-rtmps");

            TypedObject ret = new TypedObject("flex.messaging.messages.RemotingMessage");
            ret.Add("operation", operation);
            ret.Add("source", null);
            ret.Add("timestamp", 0);
            ret.Add("messageId", RTMPSEncoder.RandomUID());
            ret.Add("timeToLive", 0);
            ret.Add("clientId", null);
            ret.Add("destination", destination);
            ret.Add("body", body);
            ret.Add("headers", headers);

            return ret;
        }

        protected int NextInvokeID()
        {
            return invokeID++;
        }

        #endregion Send Methods

        #region Receive Methods

        private void MessageReceived(object messageBody)
        {
            if (OnMessageReceived != null)
                OnMessageReceived(this, messageBody);
        }

        private void BeginReceive()
        {
            decodeThread = new Thread(() =>
            {
                try
                {
                    Dictionary<int, Packet> previousReceivedPacket = new Dictionary<int, Packet>();
                    Dictionary<int, Packet> currentPackets = new Dictionary<int, Packet>();

                    while (true)
                    {
                        #region Basic Header

                        byte basicHeader = (byte)sslStream.ReadByte();
                        List<byte> basicHeaderStorage = new List<byte>();
                        if ((int)basicHeader == 255)
                            Disconnect();

                        int channel = 0;
                        //1 Byte Header
                        if ((basicHeader & 0x03) != 0)
                        {
                            channel = basicHeader & 0x3F;
                            basicHeaderStorage.Add(basicHeader);
                        }
                        //2 Byte Header
                        else if ((basicHeader & 0x01) != 0)
                        {
                            byte byte2 = (byte)sslStream.ReadByte();
                            channel = 64 + byte2;
                            basicHeaderStorage.Add(basicHeader);
                            basicHeaderStorage.Add(byte2);
                        }
                        //3 Byte Header
                        else if ((basicHeader & 0x02) != 0)
                        {
                            byte byte2 = (byte)sslStream.ReadByte();
                            byte byte3 = (byte)sslStream.ReadByte();
                            basicHeaderStorage.Add(basicHeader);
                            basicHeaderStorage.Add(byte2);
                            basicHeaderStorage.Add(byte3);
                            channel = 64 + byte2 + (256 * byte3);
                        }

                        #endregion Basic Header

                        #region Message Header

                        int headerType = (basicHeader & 0xC0);
                        int headerSize = 0;
                        if (headerType == 0x00)
                            headerSize = 12;
                        else if (headerType == 0x40)
                            headerSize = 8;
                        else if (headerType == 0x80)
                            headerSize = 4;
                        else if (headerType == 0xC0)
                            headerSize = 0;

                        // Retrieve the packet or make a new one
                        if (!currentPackets.ContainsKey(channel))
                        {
                            currentPackets.Add(channel, new Packet());
                        }

                        Packet p = currentPackets[channel];
                        p.AddToRaw(basicHeaderStorage.ToArray());

                        if (headerSize == 12)
                        {
                            //Timestamp
                            byte[] timestamp = new byte[3];
                            for (int i = 0; i < 3; i++)
                            {
                                timestamp[i] = (byte)sslStream.ReadByte();
                                p.AddToRaw(timestamp[i]);
                            }

                            //Message Length
                            byte[] messageLength = new byte[3];
                            for (int i = 0; i < 3; i++)
                            {
                                messageLength[i] = (byte)sslStream.ReadByte();
                                p.AddToRaw(messageLength[i]);
                            }
                            int size = 0;
                            for (int i = 0; i < 3; i++)
                                size = size * 256 + (messageLength[i] & 0xFF);
                            p.SetSize(size);

                            //Message Type
                            int messageType = sslStream.ReadByte();
                            p.AddToRaw((byte)messageType);
                            p.SetType(messageType);

                            //Message Stream ID
                            byte[] messageStreamID = new byte[4];
                            for (int i = 0; i < 4; i++)
                            {
                                messageStreamID[i] = (byte)sslStream.ReadByte();
                                p.AddToRaw(messageStreamID[i]);
                            }
                        }
                        else if (headerSize == 8)
                        {
                            //Timestamp
                            byte[] timestamp = new byte[3];
                            for (int i = 0; i < 3; i++)
                            {
                                timestamp[i] = (byte)sslStream.ReadByte();
                                p.AddToRaw(timestamp[i]);
                            }

                            //Message Length
                            byte[] messageLength = new byte[3];
                            for (int i = 0; i < 3; i++)
                            {
                                messageLength[i] = (byte)sslStream.ReadByte();
                                p.AddToRaw(messageLength[i]);
                            }
                            int size = 0;
                            for (int i = 0; i < 3; i++)
                                size = size * 256 + (messageLength[i] & 0xFF);
                            p.SetSize(size);

                            //Message Type
                            int messageType = sslStream.ReadByte();
                            p.AddToRaw((byte)messageType);
                            p.SetType(messageType);
                        }
                        else if (headerSize == 4)
                        {
                            //Timestamp
                            byte[] timestamp = new byte[3];
                            for (int i = 0; i < 3; i++)
                            {
                                timestamp[i] = (byte)sslStream.ReadByte();
                                p.AddToRaw(timestamp[i]);
                            }

                            if (p.GetSize() == 0 && p.GetPacketType() == 0)
                            {
                                if (previousReceivedPacket.ContainsKey(channel))
                                {
                                    p.SetSize(previousReceivedPacket[channel].GetSize());
                                    p.SetType(previousReceivedPacket[channel].GetPacketType());
                                }
                            }
                        }
                        else if (headerSize == 0)
                        {
                            if (p.GetSize() == 0 && p.GetPacketType() == 0)
                            {
                                if (previousReceivedPacket.ContainsKey(channel))
                                {
                                    p.SetSize(previousReceivedPacket[channel].GetSize());
                                    p.SetType(previousReceivedPacket[channel].GetPacketType());
                                }
                            }
                        }

                        #endregion Message Header

                        #region Message Body

                        //DefaultChunkSize is 128
                        for (int i = 0; i < 128; i++)
                        {
                            byte b = (byte)sslStream.ReadByte();
                            p.Add(b);
                            p.AddToRaw(b);

                            if (p.IsComplete())
                                break;
                        }

                        if (!p.IsComplete())
                            continue;

                        if (previousReceivedPacket.ContainsKey(channel))
                            previousReceivedPacket.Remove(channel);

                        previousReceivedPacket.Add(channel, p);

                        if (currentPackets.ContainsKey(channel))
                            currentPackets.Remove(channel);

                        #endregion Message Body

                        // Decode result
                        TypedObject result;
                        RTMPSDecoder decoder = new RTMPSDecoder();
                        if (p.GetPacketType() == 0x14) // Connect
                            result = decoder.DecodeConnect(p.GetData());
                        else if (p.GetPacketType() == 0x11) // Invoke
                            result = decoder.DecodeInvoke(p.GetData());
                        else if (p.GetPacketType() == 0x06) // Set peer bandwidth
                        {
                            byte[] data = p.GetData();
                            int windowSize = 0;
                            for (int i = 0; i < 4; i++)
                                windowSize = windowSize * 256 + (data[i] & 0xFF);
                            int type = data[4];
                            continue;
                        }
                        else if (p.GetPacketType() == 0x05) // Window Acknowledgement Size
                        {
                            byte[] data = p.GetData();
                            int windowSize = 0;
                            for (int i = 0; i < 4; i++)
                                windowSize = windowSize * 256 + (data[i] & 0xFF);
                            continue;
                        }
                        else if (p.GetPacketType() == 0x03) // Ack
                        {
                            byte[] data = p.GetData();
                            int ackSize = 0;
                            for (int i = 0; i < 4; i++)
                                ackSize = ackSize * 256 + (data[i] & 0xFF);
                            continue;
                        }
                        else if (p.GetPacketType() == 0x02) //ABORT
                        {
                            byte[] data = p.GetData();
                            continue;
                        }
                        else if (p.GetPacketType() == 0x01) //MaxChunkSize
                        {
                            byte[] data = p.GetData();
                            continue;
                        }
                        else
                        // Skip most messages
                        {
                            continue;
                        }

                        // Store result
                        int? id = result.GetInt("invokeId");

                        //Check to see if the result is valid.
                        //If it isn't, give an error and remove the callback if there is one.
                        if (result["result"].Equals("_error"))
                        {
                            Error(GetErrorMessage(result), GetErrorCode(result), ErrorType.Receive);
                        }

                        if (result["result"].Equals("receive"))
                        {
                            if (result.GetTO("data") != null)
                            {
                                TypedObject to = result.GetTO("data");
                                if (to.ContainsKey("body"))
                                {
                                    if (to["body"] is TypedObject)
                                    {
                                        new Thread(new ThreadStart(() =>
                                        {
                                            TypedObject body = (TypedObject)to["body"];

                                            if (body.type.Equals("com.riotgames.platform.game.GameDTO"))
                                                MessageReceived(new GameDTO(body));

                                            else if (body.type.Equals("com.riotgames.platform.game.PlayerCredentialsDto"))
                                                MessageReceived(new PlayerCredentialsDto(body));

                                            else if (
                                                body.type.Equals("com.riotgames.platform.gameinvite.contract.InvitationRequest"))
                                                MessageReceived(new InvitationRequest(body));

                                            else if (
                                                body.type.Equals("com.riotgames.platform.gameinvite.contract.InvitationRequest"))
                                                MessageReceived(new Inviter(body));

                                            else if (
                                                body.type.Equals("com.riotgames.platform.serviceproxy.dispatch.LcdsServiceProxyResponse"))
                                                MessageReceived(new LcdsServiceProxyResponse(body));

                                            else if (
                                                body.type.Equals(
                                                    "com.riotgames.platform.game.message.GameNotification"))
                                                MessageReceived(new GameNotification(body));
                                            else if (
                                                body.type.Equals(
                                                    "com.riotgames.platform.matchmaking.SearchingForMatchNotification"))
                                                MessageReceived(new SearchingForMatchNotification(body));
                                            else if (
                                                body.type.Equals(
                                                    "com.riotgames.platform.broadcast.BroadcastNotification"))
                                                MessageReceived(new BroadcastNotification(body));
                                            else if (
                                                body.type.Equals(
                                                    "com.riotgames.platform.messaging.StoreAccountBalanceNotification"))
                                                MessageReceived(new StoreAccountBalanceNotification(body));
                                            else if (
                                                body.type.Equals(
                                                    "com.riotgames.platform.messaging.persistence.SimpleDialogMessage"))
                                                MessageReceived(new SimpleDialogMessage(body));
                                            else if (
                                                body.type.Equals(
                                                "com.riotgames.platform.trade.api.contract.TradeContractDTO"))
                                                MessageReceived(new TradeContractDTO(body));
                                            else if (
                                                body.type.Equals(
                                                "com.riotgames.platform.statistics.EndOfGameStats"))
                                                MessageReceived(new EndOfGameStats(body));
                                            else if (
                                                body.type.Equals(
                                                "com.riotgames.platform.gameinvite.contract.LobbyStatus"))
                                                MessageReceived(new LobbyStatus(body));
                                            
                                            //MessageReceived(to["body"]);
                                        })).Start();
                                    }
                                }
                            }
                            //MessageReceived(
                        }

                        if (id == null)
                            continue;

                        if (id == 0)
                        {
                        }
                        else if (callbacks.ContainsKey((int)id))
                        {
                            RiotGamesObject cb = callbacks[(int)id];
                            callbacks.Remove((int)id);
                            if (cb != null)
                            {
                                TypedObject messageBody = result.GetTO("data").GetTO("body");
                                new Thread(() => { cb.DoCallback(messageBody); }).Start();
                            }
                        }

                        else
                        {
                            results.Add((int)id, result);
                        }

                        pendingInvokes.Remove((int)id);
                    }
                }
                catch (Exception e)
                {
                    if (IsConnected())
                        Error(e.Message, ErrorType.Receive);

                    //Disconnect();
                }
            });
            decodeThread.IsBackground = true;
            decodeThread.Start();
        }

        private TypedObject GetResult(int id)
        {
            while (IsConnected() && !results.ContainsKey(id))
            {
                Thread.Sleep(10);
            }

            if (!IsConnected())
                return null;

            TypedObject ret = results[id];
            results.Remove(id);
            return ret;
        }

        private TypedObject PeekResult(int id)
        {
            if (results.ContainsKey(id))
            {
                TypedObject ret = results[id];
                results.Remove(id);
                return ret;
            }
            return null;
        }

        private void Join()
        {
            while (pendingInvokes.Count > 0)
            {
                Thread.Sleep(10);
            }
        }

        private void Join(int id)
        {
            while (IsConnected() && pendingInvokes.Contains(id))
            {
                Thread.Sleep(10);
            }
        }

        private void Cancel(int id)
        {
            // Remove from pending invokes (only affects join())
            pendingInvokes.Remove(id);

            // Check if we've already received the result
            if (PeekResult(id) != null)
                return;
            // Signify a cancelled invoke by giving it a null callback
            else
            {
                callbacks.Add(id, null);

                // Check for race condition
                if (PeekResult(id) != null)
                    callbacks.Remove(id);
            }
        }

        #endregion Receive Methods

        #region Public Client Methods

        #endregion Public Client Methods

        #region General Returns

        public bool IsConnected()
        {
            return isConnected;
        }

        public bool IsLoggedIn()
        {
            return isLoggedIn;
        }

        #endregion General Returns
    }
}