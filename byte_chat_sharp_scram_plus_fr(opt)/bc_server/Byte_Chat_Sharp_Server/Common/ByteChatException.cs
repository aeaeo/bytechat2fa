using System;

namespace Byte_Chat_Sharp_Server.Common
{
    /// <summary>
    /// Byte chat exception class
    /// </summary>
    public class ByteChatException : Exception
    {
        /// <summary>
        /// Byte chat exception constructor
        /// It's log to file
        /// </summary>
        /// <param name="message"></param>
        /// <param name="additionals"></param>
        public ByteChatException(string message, string additionals = "") : base(message)
        {
            ErrorLogger.LogFile(new Exception(message), additionals);
        }
    }
}
