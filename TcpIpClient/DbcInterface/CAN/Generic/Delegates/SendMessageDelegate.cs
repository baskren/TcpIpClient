using System;

namespace Aptiv.Messaging
{
    /// <summary>
    /// A delegate type which Sends a Message which inherits from the IMessage
    /// interface.
    /// </summary>
    /// <param name="message">The message to send.</param>
    [Serializable]
    public delegate void SendMessageDelegate(IMessage message);

    /// <summary>
    /// A delegate type which Sends a Message of a specified type.
    /// </summary>
    /// <param name="message">The message to send.</param>
    [Serializable]
    public delegate void SendMessageDelegate<TMessage>(TMessage message) where TMessage : IMessage;
}
