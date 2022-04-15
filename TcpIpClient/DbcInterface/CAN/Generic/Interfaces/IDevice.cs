using Aptiv.Messaging;
using System;

namespace Aptiv.Devices
{
    /// <summary>
    /// Descibes a generic device interface which sends and receives IMessage's.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to send and receive.</typeparam>
    public interface IDevice<TMessage>
        where TMessage : IMessage
    {
        /// <summary>
        /// The identifier for this device.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Returns true when this device is able to send and receive messages.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Instructs the device to connect.
        /// </summary>
        /// <returns>True if the device has successfully connected.</returns>
        bool Connect();

        /// <summary>
        /// Instructs the device to disconnect.
        /// </summary>
        /// <returns>True if the device has successfully disconnected.</returns>
        bool Disconnect();

        /// <summary>
        /// Sends a message to the device
        /// </summary>
        /// <param name="message"></param>
        void Send(TMessage message);

        /// <summary>
        /// Obtains the handle which calls functions when this device receives
        /// messages.
        /// </summary>
        event EventHandler<MessageReceivedArgs<TMessage>> Handle;
    }
}
