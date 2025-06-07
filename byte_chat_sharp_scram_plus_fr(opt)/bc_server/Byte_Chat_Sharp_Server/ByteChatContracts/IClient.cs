namespace Byte_Chat_Sharp_Server.ByteChatContracts
{
    /// <summary>
    /// IClient interface of client classes
    /// </summary>
    public interface IClient
    {
        /// <summary>
        /// Send message with length 1024 bytes
        /// </summary>
        /// <param name="message"></param>
        void SendMessage(string message);

        /// <summary>
        /// Disconnect client
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Client Name
        /// </summary>
        string Name { get; }
    }
}
