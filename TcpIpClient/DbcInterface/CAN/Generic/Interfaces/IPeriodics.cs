using System.Collections.Generic;

namespace Aptiv.Messaging
{
    /// <summary>
    /// Descibes a collection of messages which are sent periodically.
    /// </summary>
    /// <typeparam name="TMessage">The type of messages being sent.</typeparam>
    public interface IPeriodics<TMessage> : ICollection<TMessage>
        where TMessage : IMessage
    {
    }
}
