using Aptiv.Messaging;
using DataBuffers;
using System;
using System.Collections.Generic;

namespace Aptiv.Devices
{
    /// <summary>
    /// Provides a stream context for IDevices.
    /// </summary>
    public class DeviceStream<TMessage> : IDevice<TMessage>, IBuffer<TMessage>, IDisposable
        where TMessage : IMessage
    {
        private IDevice<TMessage> device;
        private IBuffer<TMessage> buffer;

        /// <summary>
        /// The queue that handles sending to the device from multiple threads.
        /// All send calls should go through the Write function.
        /// </summary>
        private MessageQueue<TMessage> sendQueue;

        /// <summary>
        /// Construct a new device stream context.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="initial_buffer_size"></param>
        public DeviceStream(IDevice<TMessage> device, int initial_buffer_size = 256)
        {
            this.device = device;
            buffer = new SequentialCircularBuffer<TMessage>(initial_buffer_size);

            device.Handle += (sender, args) =>
            {
                buffer.Add(args.Received);
            };

            sendQueue = new MessageQueue<TMessage>(device.Send, device.Name + " Buffer");
        }

        /// <summary>
        /// Writes a message to this device.
        /// </summary>
        /// <param name="m">The message to write.</param>
        /// <param name="delay"></param>
        public void Write(TMessage m, int delay = 1)
        {
            sendQueue.EnqueueTask(m, delay);
        }

        #region --- IDevice ---
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<MessageReceivedArgs<TMessage>> Handle
        {
            add
            {
                device.Handle += value;
            }

            remove
            {
                device.Handle -= value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public string Name => device.Name;
        /// <summary>
        /// 
        /// </summary>
        public bool IsConnected => device.IsConnected;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            return device.Connect();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Disconnect()
        {
            return device.Disconnect();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void Send(TMessage message)
        {
            device.Send(message);
        }
        #endregion

        #region --- IBuffer ---
        /// <summary>
        /// 
        /// </summary>
        public int Size { get => buffer.Size; set => buffer.Size = value; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="toAdd"></param>
        public void Add(TMessage toAdd)
        {
            buffer.Add(toAdd);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public TMessage GetNextItem(IUniqueReader reader)
        {
            return buffer.GetNextItem(reader);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool TryGetNextItem(IUniqueReader reader, out TMessage data)
        {
            return buffer.TryGetNextItem(reader, out data);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool TryPeakItem(IUniqueReader reader, out TMessage data)
        {
            return buffer.TryPeakItem(reader, out data);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public IEnumerable<TMessage> IterateToCurrentElement(IUniqueReader reader, int max)
        {
            return buffer.IterateToCurrentElement(reader, max);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public IUniqueReader GetNewReaderIdentifier(IReaderOptions options)
        {
            return buffer.GetNewReaderIdentifier(options);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="copy"></param>
        /// <returns></returns>
        public IUniqueReader GetNewReaderIdentifier(IUniqueReader copy)
        {
            return buffer.GetNewReaderIdentifier(copy);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="max"></param>
        /// <param name="receivingMessages"></param>
        /// <returns></returns>
        public IEnumerable<TMessage> IterateFromCurrentBackwards(int max, bool receivingMessages)
        {
            return buffer.IterateFromCurrentBackwards(max, receivingMessages);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="toDelete"></param>
        public void RemoveReader(IUniqueReader toDelete)
        {
            buffer.RemoveReader(toDelete);
        }
        #endregion

        #region --- IDisposable ---
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    sendQueue.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        /// <summary>
        /// Dispose of this DeviceBuffer.
        /// </summary>
        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DeviceBuffer() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}
