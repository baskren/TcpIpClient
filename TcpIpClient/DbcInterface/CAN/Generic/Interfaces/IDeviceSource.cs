using Aptiv.Messaging;
using System.Collections.Generic;

namespace Aptiv.Devices
{
    /// <summary>
    /// An interface for an IDevice source.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public interface IDeviceSource<TMessage>
        where TMessage : IMessage
    {
        /// <summary>
        /// Obtains the collection of devices available on this source.
        /// </summary>
        IReadOnlyList<IDevice<TMessage>> Devices { get; }

        /// <summary>
        /// Returns true if the source is connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Attempts to connect to the device source.
        /// </summary>
        /// <param name="connectee">The identifier of the connectee.</param>
        /// <returns>True on success.</returns>
        bool Connect(string connectee);

        /// <summary>
        /// Attempts to disconnect from the device source.
        /// </summary>
        /// <param name="disconnectee">The identifier of the disconnectee.</param>
        /// <returns>True on success.</returns>
        bool Disconnect(string disconnectee);
    }
}
