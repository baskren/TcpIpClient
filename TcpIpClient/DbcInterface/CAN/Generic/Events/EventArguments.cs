using System;

namespace Aptiv.Messaging
{
    /// <summary>
    /// Event Args thrown when a message is received.
    /// </summary>
    [Serializable]
    public sealed class MessageReceivedArgs
    {
        /// <summary>
        /// The message received which prompted the event.
        /// </summary>
        public IMessage Received;

        /// <summary>
        /// Construct a new instance with the received message.
        /// </summary>
        /// <param name="received"></param>
        public MessageReceivedArgs(IMessage received) : base()
        {
            Received = received;
        }
    }

    /// <summary>
    /// Event Args thrown when a message is received.
    /// </summary>
    [Serializable]
    public sealed class MessageReceivedArgs<TMessage> : EventArgs
        where TMessage : IMessage
    {
        /// <summary>
        /// The message received which prompted the event.
        /// </summary>
        public TMessage Received;

        /// <summary>
        /// Construct a new instance with the received message.
        /// </summary>
        /// <param name="received"></param>
        public MessageReceivedArgs(TMessage received) : base()
        {
            Received = received;
        }
    }
}
