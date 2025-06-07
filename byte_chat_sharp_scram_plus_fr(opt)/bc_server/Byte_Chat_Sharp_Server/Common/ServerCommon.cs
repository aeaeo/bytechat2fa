using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Byte_Chat_Sharp_Server.Common
{
    public static class ServerCommon
    {
        public static IPEndPoint GetServerEndPoint()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Yellow;

                Console.Write("Specify the server port to open: ");
                string strPort = Console.ReadLine();
                if (strPort != null)
                {
                    Int32 port = Int32.Parse(strPort);

                    var endPoint = new IPEndPoint(IPAddress.Any, port);
                    return endPoint;
                }

                Console.ForegroundColor = CommonConstants.DefaultConsoleColor;
            }
            catch (Exception exception)
            {
                ErrorLogger.LogConsoleAndFile(exception);
            }
            return null;
        }

        public static void ShowServerIpInfo()
        {
            
            var host = Dns.GetHostEntry(Dns.GetHostName());

            if (host.AddressList.Length > 0)
            {
                ConsoleColor currentColor = Console.ForegroundColor;

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Your local IP addresses: ");

                Console.ForegroundColor = ConsoleColor.Cyan;
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        Console.WriteLine($"\t {ip}");
                    }
                }

                Console.WriteLine($"Your Internet IP address:\n\t {GetInternalIp()}");

                Console.ForegroundColor = CommonConstants.DefaultConsoleColor;
                Console.ForegroundColor = currentColor;
            }
        }

        private static string GetInternalIp()
        {

            try
            {
                var request = WebRequest.Create("http://geoip.hidemyass.com/");
                var responseStream = request.GetResponse().GetResponseStream();

                if (responseStream != null)
                {
                    var reader = new StreamReader(responseStream);

                    string html = reader.ReadToEnd();

                    var regex = new Regex("\\d+\\.\\d+\\.\\d+\\.\\d+");

                    var matches = regex.Matches(html);

                    foreach (Match match in matches)
                    {
                        return match.Value;
                    }

                    reader.Close();
                    reader.Dispose();
                    responseStream.Close();
                    responseStream.Dispose();
                }
            }
            catch
            {
                return "Error: Can't get external IP";
            }
            return "Can't get external IP";
        }
    }
}
