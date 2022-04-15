using Aptiv.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace Aptiv.Devices
{
    /// <summary>
    /// 
    /// </summary>
    public static class IDeviceSourceAsyncExtensions
    {
        /// <summary>
        /// Instructs the device source to connect asynchronously and returns once
        /// the reply is received.
        /// </summary>
        /// <param name="deviceSource">The device source to call Connect on.</param>
        /// <param name="name">The name to pass to the source.</param>
        /// <returns>True for success.</returns>
        public static async Task<bool> ConnectAsync<TMessage>(this IDeviceSource<TMessage> deviceSource, string name)
            where TMessage : IMessage
        {
            return await Task.Run(() => { return deviceSource.Connect(name); });
        }

        /// <summary>
        /// Instructs the device source to connect asynchronously and returns once
        /// the reply is received.
        /// </summary>
        /// <param name="deviceSource">The device source to call Connect on.</param>
        /// <param name="name">The name to pass to the source.</param>
        /// <param name="token">The token to observe.</param>
        /// <returns>True for success.</returns>
        public static async Task<bool> ConnectAsync<TMessage>(this IDeviceSource<TMessage> deviceSource, string name, CancellationToken token)
            where TMessage : IMessage
        {
            return await Task.Run(() => { return deviceSource.Connect(name); }, token);
        }
    }
}
