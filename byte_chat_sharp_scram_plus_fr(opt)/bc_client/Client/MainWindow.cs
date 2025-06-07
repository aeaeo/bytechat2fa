using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Security.Cryptography;

using Client.Common;

namespace Client
{
    //
    //
    //
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            try
            {
                // blocking fields & buttons
                ShowMessages.Enabled = false;
                SendButton.Enabled = false;
                InputMessages.Enabled = false;

                // checking for settings
                if (!File.Exists(@"Settings.inf")) {
                    SettingsForm setingsOpen = new SettingsForm();

                    setingsOpen.ShowDialog();
                }

                // show current connection info
                ShowCurrentInfo();
            }
            catch (Exception ex)
            {
                Error_Info errBox = new Error_Info(ex.Message);
                errBox.ShowDialog();
            }
        }

        static private Socket sock;
        private IPAddress ipvXaddress;
        private int selected_port;

        // show current connection info
        private void ShowCurrentInfo()
        {
            try
            {
                ShowMessages.Text = "";
                if (!File.Exists(@"Settings.inf")) {
                    throw (new Exception("Settings file not found."));
                }
                // reading settings file
                StreamReader settings = new StreamReader(@"Settings.inf");
                string buffer = settings.ReadToEnd();
                settings.Close();

                // parsing ip and port from settings file
                string[] ipAndPort = buffer.Split(':');
                if (ipAndPort.Count() != 2) {
                    throw (new Exception("Settings file is invalid."));
                }

                IPAddress[] DNSToIP = Dns.GetHostAddresses(ipAndPort[0]);
                for (int i = 0; i < DNSToIP.Length; i++) {
                    if (DNSToIP[i].AddressFamily == AddressFamily.InterNetwork) {
                        ipvXaddress = DNSToIP[i];
                    }
                }

                if (ipvXaddress == null)
                    throw (new Exception("IP Address working on wrong protocol family."));

                selected_port = int.Parse(ipAndPort[1]);
                
                int GetAddressFamilyInt() => (ipvXaddress.AddressFamily.ToString() == "InterNetwork6" ? 6 : 4);

                ShowMessages.Text += "Server IPv" + GetAddressFamilyInt() + ": "+ ipAndPort[0] + "\nPort: " + ipAndPort[1] + '\n';
            }
            catch (Exception ex)    // file not exists
            {
                ShowMessages.Text += "Connection settings failed to load!" + '\n';
                throw (ex);
            }
        }

        /*
         * Control handlers
         */

        private void SendButton_Click(object sender, EventArgs e)
        {
            try
            {
                string sendMessage = InputMessages.Text;
                byte[] sendBytes = Encoding.UTF8.GetBytes(sendMessage);
                if (sendBytes.Length < CommonConstants.MessageLength)
                    sock.Send(sendBytes);
                else
                {
                    Array.Resize(ref sendBytes, CommonConstants.MessageLength);
                    sock.Send(sendBytes);
                }
                InputMessages.Text = "";

            }
            catch (Exception ex)
            {
                Error_Info errBox = new Error_Info(ex.Message);
                errBox.ShowDialog();
                // critical error: restarting app
                Application.Restart();
            }
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            try
            {
                SettingsForm setingsOpen = new SettingsForm();

                setingsOpen.ShowDialog();

                ShowCurrentInfo();
            }
            catch (Exception ex)
            {

                Error_Info errBox = new Error_Info(ex.Message);
                errBox.ShowDialog();
            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        void ReceiveMessages(object obj)
        {
            try
            {
                while (true)
                {
                    byte[] receivedData = new byte[CommonConstants.MessageLength];
                    Array.Clear(receivedData, 0, receivedData.Length);
                    sock.Receive(receivedData);
                    Invoke((MethodInvoker)delegate ()
                    {
                        ShowMessages.AppendText('\n' + Encoding.UTF8.GetString(receivedData));
                        ShowMessages.ScrollToCaret();
                    });
                }
            }
            catch (Exception ex)
            {
                Error_Info errBox = new Error_Info(ex.Message);
                errBox.ShowDialog();
                // critical error: restarting app
                Application.Restart();
            }
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (NickBox.Text.Length > 20) throw (new Exception("Name is too long"));
                if (NickBox.Text.Length < 1) throw (new Exception("Name is too short"));
                if (InputPasswdBox.Text.Length < 1) throw (new Exception("Password input value is invalid"));

                sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sock.Connect(ipvXaddress, selected_port);   // establishing a connection to dest - server

                sock.Send(Encoding.UTF8.GetBytes(NickBox.Text));    // 1. so first we're sending plain name

                byte[] authData = new byte[CommonConstants.MessageLength];  // 2. buffer for salt & challenge
                int bytesRead = sock.Receive(authData); // recieve them

                int saltLength = authData[0];
                byte[] salt = new byte[saltLength]; // 3. unpacking salt
                Array.Copy(authData, 1, salt, 0, saltLength);

                int challengeLength = authData[saltLength + 1];
                byte[] challenge = new byte[challengeLength];   // 4. unpacking challenge
                Array.Copy(authData, saltLength + 2, challenge, 0, challengeLength);

                byte[] passwordHash = HashPassword(InputPasswdBox.Text, salt, CommonConstants.IterationCount);
                byte[] proof = GenerateProof(passwordHash, challenge);

                sock.Send(proof);   // sending proof

                var receiveThread = new Thread(ReceiveMessages) { IsBackground = true };    // launching 
                receiveThread.Start();

                //blocking some button & fields
                NickBox.Enabled = false;
                InputPasswdBox.Enabled = false;
                ConnectToChatButton.Enabled = false;
                RegisterButton.Enabled = false;
                ShowMessages.Enabled = true;
                SendButton.Enabled = true;
                InputMessages.Enabled = true;
            }
            catch (Exception ex)
            {
                NickBox.Text = "";
                InputPasswdBox.Text = "";
                Error_Info errBox = new Error_Info(ex.Message);
                errBox.ShowDialog();
            }
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (NickBox.Text.Length > 20) throw (new Exception("Name is too long"));
                if (NickBox.Text.Length < 1) throw (new Exception("Name is too short"));
                if (InputPasswdBox.Text.Length < 1) throw (new Exception("Password input value is invalid"));

                sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sock.Connect(ipvXaddress, selected_port);   // establishing a connection to dest - server

                byte[] newSalt = GenerateSalt(CommonConstants.SaltSize);
                byte[] newHash = HashPassword(InputPasswdBox.Text, newSalt, CommonConstants.IterationCount);

                byte[] authData = new byte[1 + newSalt.Length + 1 + newHash.Length];  // buffer for salt & hash
                authData[0] = (byte)newSalt.Length; // [0]: salt length
                Array.Copy(newSalt, 0, authData, 1, newSalt.Length);    // [1..salt.Length]: salt
                authData[newSalt.Length + 1] = (byte)newHash.Length; // [salt.Length+1]: hash length
                Array.Copy(newHash, 0, authData, newSalt.Length + 2, newHash.Length);   // [salt.Length + 2...CommonConstants.MessageLength]: hash

                sock.Send(Encoding.UTF8.GetBytes(NickBox.Text));   // done first anytime
                sock.Send(authData);    // credentials
                //sock.Close();
            }
            catch (Exception ex)
            {
                NickBox.Text = "";
                InputPasswdBox.Text = "";
                Error_Info errBox = new Error_Info(ex.Message);
                errBox.ShowDialog();
            }
        }

        private void EnterSending(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                if (InputMessages.Text[InputMessages.Text.Length - 1] == '\n')
                    InputMessages.Text = InputMessages.Text.Substring(0, InputMessages.Text.Length - 1);
                SendButton_Click(null, null);
            }
        }

        // SCRAM
        /// <summary>
        /// computes hash using password
        /// </summary>
        private byte[] HashPassword(string password, byte[] salt, int iterations)
        {
            using (var PBKDF2 = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                return PBKDF2.GetBytes(CommonConstants.HashSize);
            }
        }
        /// <summary>
        /// generates a password knowledge proof based on challenge
        /// </summary>
        private byte[] GenerateProof(byte[] passwordHash, byte[] challenge)
        {
            using (var hmac = new HMACSHA256(passwordHash))
            {
                return hmac.ComputeHash(challenge);
            }
        }

        private static byte[] GenerateSalt(int size)
        {
            var salt = new byte[size];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }
    }
}
