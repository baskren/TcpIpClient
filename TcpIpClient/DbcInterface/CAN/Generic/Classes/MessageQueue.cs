using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Aptiv.Messaging
{
    /// <summary>
    /// This class handles sending ANY message to the Saint2 in an asynchronous
    /// fashion. When a message is queued, it will be sent from another thread.
    /// This allows the message to have a delay after it is sent. Mutiple
    /// instances of this class are created. Periodic messages should have its
    /// own instance of this class. This class has a event called ThreadStatus
    /// which can be tied to an indicator to signal the user that the class is
    /// sending messages. In order to close the application, all instances of
    /// this class MUST be disposed.
    /// </summary>
    [Serializable]
    public class MessageQueue<TMessage> : IDisposable
        where TMessage : IMessage
    {
        /// <summary>
        ///  Increase this int if program latency is experienced on calls to EnqueueTask(Message).
        /// </summary>
        private readonly int circular_buffer_size = 128; // BlockingCollection is implemented as a Circular Buffer
        [NonSerialized]
        private Thread _worker; //worker thread
        [NonSerialized]
        private BlockingCollection<QueueTask> _tasks; // task queue
        [NonSerialized]
        private CancellationTokenSource _ts; // indicates imminent destruction
        private readonly SendMessageDelegate<TMessage> Sender; // called to send messages
        private const int time_to_dispose = 300;

        /// <summary>
        /// A status tracker enum.
        /// </summary>
        [Serializable]
        public enum ThreadStatusValue
        {
            /// <summary>
            /// The thread is busy sending messages.
            /// </summary>
            Busy = 1,
            /// <summary>
            /// The thread is waiting for more messages to send.
            /// </summary>
            Waiting = 2
        }

        /// <summary>
        /// A handler type for the event when the status of
        /// the sending thread changes.
        /// </summary>
        public event EventHandler ThreadStatusChanged;

        /// <summary>
        /// The constuctor for the MessageQueue class.
        /// </summary>
        /// <param name="sender">The delegate for directly sending messages over the Saint2 device.</param>
        /// <param name="ThreadName">The name to be given to the Sending Thread.</param>
        public MessageQueue(SendMessageDelegate<TMessage> sender, string ThreadName = "")
        {
            _tasks = new BlockingCollection<QueueTask>(circular_buffer_size);
            _ts = new CancellationTokenSource();

            Sender = sender;

            _worker = new Thread(ExecutionTask(_ts.Token).RunSynchronously) { Name = ThreadName };
            _worker.Start();
        }

        /// <summary>
        /// Holds the status of whether the thread is transmitting data or not.
        /// </summary>
        public ThreadStatusValue ThreadStatus { get; private set; }

        /// <summary>
        /// Adds the data and delay to the queue to be sent.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="sleep">The delay before the message is sent in milliseconds</param>
        /// <param name="purpose">The purpose to be logged.</param>
        [Obsolete("The purpose argument has been removed to improve efficiency" +
            ". Please ommit that argument.")]
        public void EnqueueTask(TMessage message, int sleep, string purpose)
        {
            _tasks.Add(new QueueTask(sleep, message));
        }

        /// <summary>
        /// Adds the data and delay to the queue to be sent.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="sleep">The delay before the message is sent in milliseconds</param>
        public void EnqueueTask(TMessage message, int sleep)
        {
            _tasks.Add(new QueueTask(sleep, message));
        }

        /// <summary>
        /// Worker Thread function.
        /// </summary>
        /// <remarks>
        /// The usual form of a Saint Program that sends messages allows the
        /// user to start and stop the sending of messages.
        /// WITH THIS IN MIND, the Execution task should expect to be paused
        /// when there are no messages to send and the user has indicated
        /// to stop sending messages. This should be handled appropriately.
        /// ADDITIONALLY, there are usually messages being sent on an interval
        /// of at most 1 second intervals, sometimes on 10ms intervals, and this
        /// scenario should also be handled appropriately.
        /// </remarks>
        private Task ExecutionTask(CancellationToken token)
        {
            return new Task(() =>
            {
                int previousSleep = 0;
                while (!token.IsCancellationRequested)
                {
                        // Notify the handle that the tasks have been depleted.
                        ThreadStatus = ThreadStatusValue.Waiting;
                    ThreadStatusChanged?.Invoke(this, new EventArgs());

                    try
                    {
                            // Wait here to avoid spinning when user pauses.
                            if (!_tasks.TryTake(out QueueTask task_, int.MaxValue, token))
                            continue;

                        task_.Execute(Sender, previousSleep);
                        previousSleep = task_.Sleep;

                            // Notify the handle that the tasks are being processed.
                            // (Do this after Execution to avoid any slowdown.)
                            ThreadStatus = ThreadStatusValue.Busy;
                        ThreadStatusChanged?.Invoke(this, new EventArgs());

                            // Get the enumerable to iterate over consecutive messages.
                            foreach (QueueTask task in _tasks.GetConsumingEnumerable(token))
                        {
                            task.Execute(Sender, previousSleep);
                            previousSleep = task.Sleep;
                        }
                    }
                    catch (TaskCanceledException) { return; }
                }
            });
        }

        /// <summary>
        /// A class for an item to be queued.
        /// </summary>
        [Serializable]
        struct QueueTask
        {
            /// <summary>
            /// The amount of time to wait after sending.
            /// </summary>
            public int Sleep;

            /// <summary>
            /// The message object to send.
            /// </summary>
            [NonSerialized]
            public TMessage Message;

            /// <summary>
            /// Construct a new queue task.
            /// </summary>
            /// <param name="sleep">The time to wait after sending.</param>
            /// <param name="m">The message to send for this Task.</param>
            public QueueTask(int sleep, TMessage m)
            {
                Sleep = sleep;
                Message = m;
            }

            /// <summary>
            /// Execute this Queue Task and send the message afterwards sleep for duration.
            /// </summary>
            public void Execute(SendMessageDelegate<TMessage> Sender, int previousSleep)
            {
                Thread.Sleep(previousSleep);
                Sender(Message);
            }
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Dispose of the cancellation token and the blocking collection.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _ts.Cancel();
                    _worker.Join(time_to_dispose);

                    _tasks.Dispose();
                    _ts.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MessageQueue() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        /// <summary>
        /// Dispose of the cancellation token and the blocking collection.
        /// </summary>
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
