using System;

namespace Aptiv.Messaging
{
    /// <summary>
    /// The delegate for sending messages with a <c>Periodics</c> instance.
    /// </summary>
    /// <param name="message"></param>
    [Serializable]
    public delegate void SendMessagePeriodicDelegate(IMessage message);

    /// <summary>
    /// The delegate for sending messages with a <c>Periodics</c> instance.
    /// </summary>
    /// <param name="message"></param>
    [Serializable]
    public delegate void SendMessagePeriodicDelegate<TMessage>(TMessage message);
}
