using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using Emgu.CV;  // FR: funcs stub until full integration

using Byte_Chat_Sharp_Server.ByteChatContracts;
using Byte_Chat_Sharp_Server.Common;

namespace Byte_Chat_Sharp_Server.ByteChatClasses
{
    /// <summary>
    /// Byte chat server class
    /// </summary>
    public static class Server
    {
        private static Thread _serverThread;
        private static readonly object _criticalSection = false;
        private static List<IClient> _clients;

        // users storage: [Name : tuple(salt, hash)]
        private static readonly Dictionary<string, (byte[] SaltedHash, byte[] Hash)> _userDatabase = new Dictionary<string, (byte[], byte[])>();
        // active auth keys storage: [Name : key]
        private static readonly Dictionary<string, byte[]> _activeAuthKeys = new Dictionary<string, byte[]>();

        // this also creates a test user
        static Server()
        {
            string username = "user";  // defaults to this
            string password = "password";
            Console.WriteLine("[DBG] Generating debug credentials...");
            byte[] salt = GenerateSalt(CommonConstants.SaltSize);
            byte[] hash = HashPassword(password, salt, CommonConstants.IterationCount);
            _userDatabase.Add(username, (salt, hash));

            // FR: face recognition cascade gets initialized here
            /*try
            {
                if (!File.Exists("haarcascade_frontalface_alt.xml"))
                {
                    Console.WriteLine("WARNING: Face cascade file not found. Face recognition will not work.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Face recognition initialization error: {ex.Message}");
            }*/

            Console.WriteLine("Starting server...");
        }

        /// <summary>
        /// Start Server Thread
        /// </summary>
        /// <param name="endPoint"></param>
        public static void StartServer(IPEndPoint endPoint)
        {
            try
            {
                if (endPoint == null)
                    throw new ByteChatException("IP End Point is null", "Start Server: ");

                Console.ForegroundColor = ConsoleColor.Yellow;

                if (_serverThread != null)
                {
                    Console.WriteLine("Stop previously launched thread...");
                    _serverThread.Abort();
                    _serverThread = null;

                    lock (_criticalSection)
                    {
                        if (_clients != null)
                        {
                            _clients.Clear();
                            _clients = null;
                        }
                    }
                }

                Console.WriteLine("Starting server thread...");
                Console.ForegroundColor = CommonConstants.DefaultConsoleColor;

                _serverThread = new Thread(ServerThreadMethod) { IsBackground = true };
                _serverThread.Start(endPoint);
            }
            catch (Exception exception)
            {
                ErrorLogger.LogConsoleAndFile(exception, "Start Server: ");
            }
        }

        // FR: Supports also face Thread states
        enum EUserThreadState { SUCCESS, FAILURE, REGISTER_ME_IM_NEW, FACE_AUTH_REQUIRED, FACE_REGISTRATION_REQUIRED }

        private static void ServerThreadMethod(object endPoint)
        {
            try
            {
                lock (_criticalSection)
                {
                    _clients = new List<IClient>();
                }
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Start end point binding...");
                // prepare end point for binding
                var localEndPoint = (IPEndPoint)endPoint;
                

                Console.WriteLine($"End point binded on Address:Port: {localEndPoint.Address}:{localEndPoint.Port}");
                

                // create a TCP/IP socket
                var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


                // binding server socket
                serverSocket.Bind(localEndPoint);
                serverSocket.Listen(10);
                Console.WriteLine("Server socket binded.");

                ServerCommon.ShowServerIpInfo();

                Console.WriteLine("Start listening...");

                Console.ForegroundColor = CommonConstants.DefaultConsoleColor;
                while (true)
                {
                    var newClientSocket = serverSocket.Accept();

                    byte[] receivedBytes = new byte[CommonConstants.MessageLength];
                    int username_receivedCount = newClientSocket.Receive(receivedBytes);    // recieving name
                    string username = Encoding.UTF8.GetString(receivedBytes, 0, username_receivedCount).TrimEnd('\0');

                    if (string.IsNullOrWhiteSpace(username))
                    {
                        Console.WriteLine("[DBG]: Server.ServerThreadMethod(): Empty username received, closing connection");
                        newClientSocket.Close();
                        continue;
                    }

                    EUserThreadState connectionState = TestAuthSaltedCRAM(newClientSocket, receivedBytes, username); // challange SCRAM

                    if (connectionState == EUserThreadState.FAILURE)
                    {
                        Console.WriteLine($"[DBG]: Server.ServerThreadMethod(): Authentication failed for {username}");
                        continue;
                    }
                    
                    if (connectionState == EUserThreadState.REGISTER_ME_IM_NEW)
                    {
                        Console.WriteLine($"[DBG]: Server.ServerThreadMethod(): Starting registration for {username}");
                        RegisterEntity(newClientSocket, receivedBytes, username);
                        continue;
                    }

                    // FR: potentially started right here
                    /*if (connectionState == EUserThreadState.FACE_AUTH_REQUIRED)
                    {
                        Console.WriteLine($"Server.ServerThreadMethod(): Face authentication required for {username}");
                        EUserThreadState faceAuthResult = HandleFaceAuthentication(newClientSocket, username);

                        if (faceAuthResult == EUserThreadState.SUCCESS)
                        {
                            HandleNewClient(newClientSocket, username);
                        }
                        else if (faceAuthResult == EUserThreadState.FACE_REGISTRATION_REQUIRED)
                        {
                            HandleFaceRegistration(newClientSocket, username);
                        }
                        else
                        {
                            Console.WriteLine($"Server.ServerThreadMethod(): Face authentication failed for {username}");
                            newClientSocket.Close();
                        }
                        continue;
                    }*/

                    if (connectionState == EUserThreadState.SUCCESS)
                    {
                        HandleNewClient(newClientSocket, username);   // handle newly created connection
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorLogger.LogConsoleAndFile(exception, "Server Thread stopping: ");
            }
            finally
            {
                _serverThread = null;

                if (_clients != null)
                {
                    lock (_criticalSection)
                    {
                        _clients.Clear();
                        _clients = null;
                    }
                }
            }
        }

        /*
         * обробка 
         */
        private static EUserThreadState TestAuthSaltedCRAM(Socket newClientSocket, byte[] receivedBytes, string username)
        {
            lock (_criticalSection)
            {
                try
                {
                    if (_userDatabase.ContainsKey(username))    // sending salt to user if he is in the list
                    {
                        var (salt, hash) = _userDatabase[username]; // getting both credentials from the db
                        byte[] challenge = GenerateChallenge(CommonConstants.ChallengeSize); // generating rndom challenge for session
                        
                        if (_activeAuthKeys.ContainsKey(username) && _activeAuthKeys[username] != challenge)   //
                        {
                            Console.WriteLine($"Server.VerifySCRAMLike(): An already active session for {username} exists. Authentication is not possible in current policy.");
                            newClientSocket.Close();
                            return EUserThreadState.FAILURE;
                        }
                        
                        _activeAuthKeys[username] = challenge;  // placing it in the slot by key

                        // </SUB: sending both salt & challenge to client
                        var authData = new byte[1 + salt.Length + 1 + challenge.Length];
                        authData[0] = (byte)salt.Length;    // [0]: salt length
                        Array.Copy(salt, 0, authData, 1, salt.Length);  // [1..salt.Length]: salt
                        authData[salt.Length + 1] = (byte)challenge.Length; // [salt.Length+1]: challenge length
                        Array.Copy(challenge, 0, authData, salt.Length + 2, challenge.Length); // [salt.Length + 2...n]: challenge

                        newClientSocket.Send(authData); // SUB>

                        // </SUB: handling client's response to 
                        int receivedCount = newClientSocket.Receive(receivedBytes);
                        var receivedProof = new byte[receivedCount];
                        Array.Copy(receivedBytes, receivedProof, receivedCount); // SUB>

                        // </SUB: verifying whether client's response is valid or not
                        // if valid, proceed
                        if (!VerifyProof(username, receivedProof)) // actually just verifying hash
                        {
                            Console.WriteLine($"Server.TestAuthSaltedSCRAM(): Authentication failed for user: {username}");
                            _activeAuthKeys.Remove(username);
                            return EUserThreadState.FAILURE;
                        }
                        //return EUserThreadState.FACE_AUTH_REQUIRED; // FR: if SCRAM is passed, require face authentication; uncomment this place after integration
                        return EUserThreadState.SUCCESS;
                    }
                    else // this user is no in db
                    {
                        Console.WriteLine($"Server.TestAuthSaltedSCRAM(): User not found: {username}");
                        return EUserThreadState.REGISTER_ME_IM_NEW;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Server.TestAuthSaltedSCRAM(): Error processing the client: {ex.Message}");
                    if (_activeAuthKeys.ContainsKey(username))
                        _activeAuthKeys.Remove(username);
                    newClientSocket.Close();
                }
                return EUserThreadState.FAILURE;
            }
        }

        private static EUserThreadState HandleFaceAuthentication(Socket clientSocket, string username)
        {
            /*FR: face auth code goes here, one may require more additional methods */
            return EUserThreadState.SUCCESS;    // stub
        }

        private static bool ProcessFaceRegistration(string username, byte[] imageData)
        {
            /*FR: face registration goes here*/
            return true;
        }

        private static double CompareFaces(Mat face1, Mat face2)
        {
            /*FR: face comparison goes here*/
            return 1.0f;
        }

        /*
         * Handle new client connection: add client to active list
         */
        private static void HandleNewClient(Socket clientSocket, string username)
        {
            string messageConnectedClient;
            string clientIP;

            lock (_criticalSection)
            {
                IClient client = new Client(clientSocket, _clients, username, _criticalSection);    // create client 
                _clients.Add(client);
                clientIP = ((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString();
                messageConnectedClient = "Connected: " + client.Name;
            }

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(messageConnectedClient + " IP: " + clientIP);
            Console.ForegroundColor = CommonConstants.DefaultConsoleColor;

            SendAll(messageConnectedClient);
        }

        public static void RegisterEntity(Socket clientSocket, byte[] receivedBytes, string username)
        {
            string messageRegisteredClient;
            string clientIP;

            try
            {
                lock (_criticalSection)
                {
                    int receivedCount = clientSocket.Receive(receivedBytes);

                    if (receivedCount < 3) // minimum sum
                    {
                        Console.WriteLine($"Server.RegisterEntity(): Invalid data received from {username}");
                        clientSocket.Close();
                        return;
                    }

                    int saltLength = receivedBytes[0];

                    if (saltLength <= 0 || saltLength > receivedCount - 2)
                    {
                        Console.WriteLine($"Server.RegisterEntity(): Invalid salt length for {username}");
                        clientSocket.Close();
                        return;
                    }

                    byte[] salt = new byte[saltLength];
                    Array.Copy(receivedBytes, 1, salt, 0, saltLength);

                    int hashLength = receivedBytes[saltLength + 1];

                    if (hashLength <= 0 || saltLength + 2 + hashLength > receivedCount)
                    {
                        Console.WriteLine($"Server.RegisterEntity(): Invalid hash length for {username}");
                        clientSocket.Close();
                        return;
                    }

                    byte[] hash = new byte[hashLength];
                    Array.Copy(receivedBytes, saltLength + 2, hash, 0, hashLength);

                    if (_userDatabase.ContainsKey(username))
                    {
                        Console.WriteLine($"Server.RegisterEntity(): User {username} already exists");
                        clientSocket.Close();
                        return;
                    }
                    _userDatabase.Add(username, (salt, hash));
                    clientIP = ((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString();
                    messageRegisteredClient = "Registered: " + username;

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(messageRegisteredClient + " IP: " + clientIP);
                    Console.ForegroundColor = CommonConstants.DefaultConsoleColor;
                }
                SendAll(messageRegisteredClient);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RegisterEntity(): Error registering user {username}: {ex.Message}");
                /*try
                {
                    byte[] errorMessage = Encoding.UTF8.GetBytes("REGISTRATION_FAILED");
                    clientSocket.Send(errorMessage);
                }
                catch
                {
                    // just ignoring the errors, it's useless to handle them
                }*/
            }
            finally // we're ANYWAY closing the connection after the registration
            {
                try
                {
                    clientSocket.Close();
                }
                catch   // TODO: handle errors, this socket connection may be dumped
                {
                }
            }
        }

        public static void SendAll(string message, IClient clientNotSend = null)
        {
            try
            {
                lock (_criticalSection)
                {
                    foreach (var client in _clients)
                    {
                        try
                        {
                            if (!client.Equals(clientNotSend))
                                client.SendMessage(message);
                        }
                        catch (Exception localException)
                        {
                            Console.WriteLine("Error sending message to " + client.Name + ": " + localException.Message);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorLogger.LogConsoleAndFile(exception); // >!-- originally commented out --!<
            }
        }

        /*
         *
         * SCRAM-specific methods
         *
         */

        /// <summary>
        /// Checks client's knowledge of session key
        /// </summary>
        private static bool VerifyProof(string username, byte[] receivedProof)
        {
            if (!_userDatabase.ContainsKey(username) || !_activeAuthKeys.ContainsKey(username)) {
                Console.WriteLine($"[DBG] Server.VerifyProof(): client proof verification failure - client is not in the DB or active");
                return false;
            }

            var (salt, storedHash) = _userDatabase[username];   //
            var challenge = _activeAuthKeys[username];

            var serverProof = GenerateProof(storedHash, challenge); // generating server proof

            bool verStatus = ByteArraysTimedEqual(receivedProof, serverProof); // comparison between server and client proofs

            Console.WriteLine($"[DBG] Server.VerifyProof(): client valid, proof status - {verStatus}");

            return verStatus;
        }

        /// <summary>
        /// Generates proof based on password hash and challenge
        /// </summary>
        private static byte[] GenerateProof(byte[] passwordHash, byte[] challenge)
        {
            using (var hmac = new HMACSHA256(passwordHash))
            {
                Console.WriteLine($"[DBG] Server.GenerateProof(): HMACSHA256-based proof generated");
                return hmac.ComputeHash(challenge);
            }
        }

        /// <summary>
        /// Hashes password with salt using PBKDF2 (RFC2898) with given salt and num of iterations
        /// </summary>
        private static byte[] HashPassword(string password, byte[] salt, int iterations)
        {
            using (var PBKDF2 = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                Console.WriteLine($"[DBG] Server.HashPassword(): {iterations} iterations PBKDF2 key generated");
                return PBKDF2.GetBytes(CommonConstants.HashSize);
            }
        }

        /// <summary>
        /// Random salt
        /// </summary>
        private static byte[] GenerateSalt(int size)
        {
            var salt = new byte[size];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            Console.WriteLine($"[DBG] Server.GenerateSalt(): {size} bytes challenge generated");
            return salt;
        }

        /// <summary>
        /// Random challenge
        /// </summary>
        private static byte[] GenerateChallenge(int size)
        {
            var challenge = new byte[size];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(challenge);
            }
            Console.WriteLine($"[DBG] Server.GenerateChallenge(): {size} bytes challenge generated");
            return challenge;
        }

        /// <summary>
        /// Timing attack-safe byte arrays comparison
        /// </summary>
        private static bool ByteArraysTimedEqual(byte[] a1, byte[] a2)
        {
            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length)
                return false;

            int result = 0;
            for (int i = 0; i < a1.Length; i++) // comparing at fixed time, so we're safe of 
            {
                result |= a1[i] ^ a2[i];
            }
            Console.WriteLine($"[DBG] Server.ByteArraysTimedEqual(): result - {result}, a1.Length - {a1.Length}, a2.Length {a2.Length}");
            Console.WriteLine($"[DBG] Server.ByteArraysTimedEqual(): test {result == 0 : true ? false}");
            return result == 0;
        }
    }
}
