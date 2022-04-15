namespace Aptiv.Messaging
{
    /// <summary>
    /// Represents a generic pattern which is compared to a message.
    /// </summary>
    public interface IPattern
    {
        /// <summary>
        /// 
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Determines if the provided message is a match of this pattern.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        bool IsMatch(IMessage message);
    }

    /// <summary>
    /// Represents a generic pattern which is compared to a message of a
    /// specified type.
    /// </summary>
    /// <typeparam name="TMessage">The specific type of IMessage to compare to.</typeparam>
    public interface IPattern<TMessage> : IPattern
        where TMessage : IMessage
    {
        /// <summary>
        /// Determines if the provided message is a match of this pattern.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        bool IsMatch(TMessage message);
    }
}
