using System;
using System.Collections;
using System.Collections.Generic;

namespace Aptiv.Messaging
{
    /// <summary>
    /// The generic interface for a message object. A message object is assumed
    /// to have some structure, of which is exposed interior elements of which
    /// each message of a defined type is composed.
    /// </summary>
    public interface IMessage : IEnumerable, IComparable
    {
    }

    /// <summary>
    /// The generic interface for a message object. A message object is assumed
    /// to have some structure, of which is exposed interior elements of which
    /// each message of a defined type is composed.
    /// </summary>
    /// <typeparam name="TComponent">The type of the message's interior
    /// elements</typeparam>
    public interface IMessage<TComponent> : IMessage, IEnumerable<TComponent>
        where TComponent : IComparable
    {
    }
}
