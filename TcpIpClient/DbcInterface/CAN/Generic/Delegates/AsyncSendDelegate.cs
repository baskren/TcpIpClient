using System.Threading.Tasks;

namespace Aptiv.Messaging.Async
{
    /// <summary>
    /// A delegate type which Sends a Message, intended
    /// for async scenarios.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <returns>An awaitable task.</returns>
    public delegate Task AsyncSendMessageDelegate(IMessage message);

    /// <summary>
    /// A delegate type which Sends a Message of a specified type, intended
    /// for async scenarios.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to send.</typeparam>
    /// <param name="message">The message to send.</param>
    /// <returns>An awaitable task.</returns>
    public delegate Task AsyncSendMessageDelegate<TMessage>(TMessage message)
        where TMessage : IMessage;
}
