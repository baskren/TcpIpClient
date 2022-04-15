using Aptiv.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace Aptiv.Devices
{
    /// <summary>
    /// Provides Async wrapper methods for Device action which take a while.
    /// </summary>
    public static class IDevice_Async_Extensions
    {
        /// <summary>
        /// Instructs the device to connect asynchronously and returns once
        /// the reply is received.
        /// </summary>
        /// <param name="device">The device to call Connect on.</param>
        /// <returns>True for success.</returns>
        public static async Task<bool> ConnectAsync<TMessage>(this IDevice<TMessage> device)
            where TMessage : IMessage
        {
            return await Task.Run(() => { return device.Connect(); });
        }

        /// <summary>
        /// Instructs the device to connect asynchronously and returns once
        /// the reply is received.
        /// </summary>
        /// <param name="device">The device to call Connect on.</param>
        /// <param name="token">The token to observe.</param>
        /// <returns>True for success.</returns>
        public static async Task<bool> ConnectAsync<TMessage>(this IDevice<TMessage> device, CancellationToken token)
            where TMessage : IMessage
        {
            return await Task.Run(() => { return device.Connect(); }, token);
        }
    }
}
