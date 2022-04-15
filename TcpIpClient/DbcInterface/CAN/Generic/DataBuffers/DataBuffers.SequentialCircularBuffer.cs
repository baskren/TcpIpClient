// DataBuffers.SequentialCircularBuffer<TItem>
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SequentialCircularBuffers;

namespace DataBuffers
{
	/// <summary>
	/// A class providing a way to insert a TItem to a data queue
	/// and later pop it out. Enfources that no data object will not be overriden
	/// until it has been read by every UniqueReader created
	/// </summary>
	/// <remarks>
	/// This class will be optimized towards making an Add as efficient as possible. Second to this is
	/// making an itteration to the current element. 
	/// If reading is not kept current, the size used for the class will expanding. When expanding, all
	/// reads are temperarily blocked if called, but writes continue (unless there are unlikely circumstances). Expanding can also be called explicitly. 
	/// The class can only expand in descrete increments of rings, set at construction.
	///
	/// Creating and removing IUniqueReaders(Indexes) is currently not a thread-safe procedure
	/// </remarks>
	/// <typeparam name="TItem">The item stored in the buffer</typeparam>
	public sealed class SequentialCircularBuffer<TItem> : IBuffer<TItem>
	{
		internal class Index : IUniqueReader
		{
			/// <summary>
			/// This gives a way of telling where we are in all the messages sent
			/// without locking to get the indexes
			/// </summary>
			/// <remarks>
			/// All operations upon this must be using interlocked
			/// </remarks>
			public long _currentMessageIndex;

			/// <summary>
			/// This stores the lock on the general index
			/// </summary>
			internal readonly object IndexLock = new object();

			/// <summary>
			/// The current index into the ring
			/// </summary>
			internal int CurrentRingIndex;

			/// <summary>
			/// The number of times this has updated to a new ring
			/// </summary>
			internal volatile int ringCount;

			/// <summary>
			/// A reference to the current ring
			/// This is so we don't have to block as much when inserting a new ring
			/// </summary>
			internal LinkedListNode<Ring<TItem>> currentRing;

			/// <summary>
			/// This is the unique message ID for easy telling where we are
			/// </summary>
			internal long CurrentMessageIndex
			{
				get
				{
					return Interlocked.Read(ref _currentMessageIndex);
				}
				set
				{
					Interlocked.Exchange(ref _currentMessageIndex, value);
				}
			}

			internal Index(LinkedListNode<Ring<TItem>> currentRing)
			{
				this.currentRing = currentRing;
			}

			internal Index(LinkedListNode<Ring<TItem>> currentRing, int RingIndex, long MessageIndex, int RingCount)
			{
				this.currentRing = currentRing;
				CurrentRingIndex = RingIndex;
				CurrentMessageIndex = MessageIndex;
				ringCount = RingCount;
			}

			internal Index(Index toCopy)
				: this(toCopy.currentRing, toCopy.CurrentRingIndex, (long)toCopy.CurrentRingIndex, toCopy.ringCount)
			{
			}
		}

		/// <summary>
		/// Stores the size each ring is set to
		/// </summary>
		private readonly int RingSize;

		/// <summary>
		/// Collection of every ring that data is stored in
		/// </summary>
		private readonly LinkedList<Ring<TItem>> chain;

		private readonly Index writeIndex;

		/// <summary>
		/// Stores the amount of readers we have right now
		/// </summary>
		/// <remarks>
		/// Lock this to 
		/// </remarks>
		private readonly List<Index> readingIndexes;

		/// <summary>
		/// Contains the task that is resizing the chain
		/// </summary>
		private volatile Task IncrementingChain;

		/// <summary>
		/// the lowest current IUniqueReader number or int max if there are not readers
		/// </summary>
		private volatile int LowestReadingRing = int.MaxValue;

		/// <summary>
		/// The current lowest ring number for the last inserted ring
		/// </summary>
		private volatile int HighestInsertedRing;

		/// <summary>
		/// The number of rings the writer has to be before a reader before it
		/// spawns a new ring
		/// </summary>
		private const int bufferRingCount = 1;

		/// <summary>
		/// The number of rings to grow at a time
		/// </summary>
		private int RingGrowthIncrement = 2;

		/// <summary>
		/// Returns the size of the data structure
		/// Setting this pauses all additions to the data structure
		/// </summary>
		public int Size
		{
			get
			{
				if (IncrementingChain != null)
				{
					IncrementingChain.Wait();
				}
				int count;
				lock (chain)
				{
					count = chain.Count;
				}
				return RingSize * count;
			}
			set
			{
				bool lockTaken = false;
				int num = value / RingSize - chain.Count + 1;
				if (num < 0)
				{
					return;
				}
				try
				{
					lock (writeIndex.IndexLock)
					{
						if (IncrementingChain != null)
						{
							IncrementingChain.Wait();
						}
						Monitor.Enter(chain, ref lockTaken);
					}
					int count = chain.Count;
					LinkedListNode<Ring<TItem>> linkedListNode = writeIndex.currentRing;
					for (int i = 0; i < num; i++)
					{
						chain.AddAfter(linkedListNode, new Ring<TItem>(RingSize));
						linkedListNode = linkedListNode.Next;
					}
					if (HighestInsertedRing > writeIndex.ringCount)
					{
						HighestInsertedRing += num;
					}
					else if (writeIndex.ringCount < count)
					{
						HighestInsertedRing = chain.Count - 1;
					}
					else
					{
						HighestInsertedRing = num + writeIndex.ringCount;
					}
				}
				finally
				{
					if (lockTaken)
					{
						Monitor.Exit(chain);
					}
				}
			}
		}

		/// <summary>
		/// Makes a SequentialCircularBuffer
		/// </summary>
		/// <param name="initialSize">The initial amount of items that the buffer should store</param>
		/// <param name="ringSize">The number of each items each ring can store</param>
		public SequentialCircularBuffer(int initialSize, int ringSize = 512)
		{
			RingSize = ringSize;
			int num = initialSize / RingSize;
			if (num < 3)
			{
				num = 3;
			}
			chain = new LinkedList<Ring<TItem>>();
			readingIndexes = new List<Index>();
			for (int i = 0; i < num; i++)
			{
				chain.AddFirst(new Ring<TItem>(RingSize));
			}
			writeIndex = new Index(chain.First);
		}

		/// <summary>
		/// Adds a item to the buffer
		/// </summary>
		/// <param name="toAdd">The item to add</param>
		public void Add(TItem toAdd)
		{
			lock (writeIndex.IndexLock)
			{
				Ring<TItem> value = writeIndex.currentRing.Value;
				int currentRingIndex = writeIndex.CurrentRingIndex;
				value[currentRingIndex] = toAdd;
				IncrementWriteIndex(writeIndex);
			}
		}

		/// <summary>
		/// Gets the next item, and spins if there is no new item available
		/// </summary>
		/// <param name="reader">The reader this is acting upon</param>
		/// <returns>The next item</returns>
		public TItem GetNextItem(IUniqueReader reader)
		{
			Index index = (Index)reader;
			lock (index.IndexLock)
			{
				long currentMessageIndex = index.CurrentMessageIndex;
				while (currentMessageIndex == writeIndex.CurrentMessageIndex)
				{
				}
				TItem result = index.currentRing.Value[index.CurrentRingIndex];
				IncrementReadIndex(index);
				return result;
			}
		}

		/// <summary>
		/// Tries to get the next item available, if it exists.
		/// </summary>
		/// <param name="reader">The reader to act upon</param>
		/// <param name="data">The data returned, if any</param>
		/// <returns></returns>
		public bool TryGetNextItem(IUniqueReader reader, out TItem data)
		{
			Index index = (Index)reader;
			lock (index.IndexLock)
			{
				long currentMessageIndex = writeIndex.CurrentMessageIndex;
				if (index._currentMessageIndex >= currentMessageIndex)
				{
					data = default(TItem);
					return false;
				}
				LinkedListNode<Ring<TItem>> currentRing = index.currentRing;
				int currentRingIndex = index.CurrentRingIndex;
				TItem val = currentRing.Value[currentRingIndex];
				IncrementReadIndex(index);
				data = val;
				return true;
			}
		}

		/// <summary>
		/// Iterates over every available element, until the last new one is read
		/// </summary>
		/// <param name="reader">The reader this is reading for</param>
		/// <param name="max">The maximum elements that are going to be read. Has to be smaller then ringSize - 1</param>
		/// <returns>An enumerable of items to read</returns>
		public IEnumerable<TItem> IterateToCurrentElement(IUniqueReader reader, int max)
		{
			Index i = (Index)reader;
			long currentMessageIndex = writeIndex.CurrentMessageIndex;
			lock (i.IndexLock)
			{
				while (currentMessageIndex != i.CurrentMessageIndex)
				{
					yield return i.currentRing.Value[i.CurrentRingIndex];
					IncrementReadIndex(i);
				}
			}
		}

		/// <summary>
		/// Iterates from the current message to the last available one
		/// </summary>
		/// <param name="max">The maximum abount of messages you want to receive</param>
		/// <param name="receivingMessages">If any messages are going to be ariving</param>
		/// <returns></returns>
		public IEnumerable<TItem> IterateFromCurrentBackwards(int max, bool receivingMessages)
		{
			Index startFrom;
			lock (writeIndex.IndexLock)
			{
				startFrom = new Index(writeIndex);
			}
			return IterateFromReaderBackwards(max, startFrom, receivingMessages);
		}

		/// <summary>
		/// Iterates from start from to the last message available
		/// This is updates StartFrom until it points to the last available reader
		/// Also not thread safe if StartFrom is updated outside this function
		/// </summary>
		/// <param name="max">the maximum amount of messages to receive</param>
		/// <param name="StartFrom">the reader to start from</param>
		/// <param name="receivingMessages">
		/// if the data structure is receiving messages
		/// this allows for me to return messages to you that usually cannot be guarenteed to not 
		/// be overriden
		/// </param>
		/// <returns></returns>
		private IEnumerable<TItem> IterateFromReaderBackwards(int max, IUniqueReader StartFrom, bool receivingMessages)
		{
			Index toGoTo = GetLastAvailableReader();
			Index currentIndex = (Index)StartFrom;
			UnsafeDecrement(currentIndex);
			int toGoToRingcount = toGoTo.ringCount;
			while (toGoToRingcount != currentIndex.ringCount && max != 0)
			{
				yield return currentIndex.currentRing.Value[currentIndex.CurrentRingIndex];
				UnsafeDecrement(currentIndex);
				max--;
			}
			int CurrentRingIndex = toGoTo.CurrentRingIndex;
			while (CurrentRingIndex != currentIndex.CurrentRingIndex && max != 0)
			{
				yield return currentIndex.currentRing.Value[currentIndex.CurrentRingIndex];
				UnsafeDecrement(currentIndex);
				max--;
			}
			if (max != 0 && currentIndex.ringCount >= 0)
			{
				yield return currentIndex.currentRing.Value[currentIndex.CurrentRingIndex];
				max--;
			}
			RemoveReader(toGoTo);
			UnsafeDecrement(currentIndex);
			if (receivingMessages || max == 0 || currentIndex.ringCount <= 0)
			{
				yield break;
			}
			while (currentIndex.ringCount != writeIndex.ringCount && currentIndex.ringCount != HighestInsertedRing && max != 0 && currentIndex.ringCount >= 0)
			{
				yield return currentIndex.currentRing.Value[currentIndex.CurrentRingIndex];
				UnsafeDecrement(currentIndex);
				max--;
			}
			if (currentIndex.ringCount != HighestInsertedRing && max != 0 && currentIndex.ringCount < 0)
			{
				while (writeIndex.CurrentRingIndex != currentIndex.CurrentRingIndex && max != 0)
				{
					yield return currentIndex.currentRing.Value[currentIndex.CurrentRingIndex];
					UnsafeDecrement(currentIndex);
					max--;
				}
				if (max != 0)
				{
					yield return currentIndex.currentRing.Value[currentIndex.CurrentRingIndex];
				}
			}
		}

		/// <summary>
		/// Peaks an item and sets it if the item exists
		/// otherwise returns false
		/// </summary>
		/// <param name="reader">the reader to act upon</param>
		/// <param name="data">the variable to set with the data</param>
		/// <returns>If data was set</returns>
		public bool TryPeakItem(IUniqueReader reader, out TItem data)
		{
			Index index = (Index)reader;
			lock (index.IndexLock)
			{
				long currentMessageIndex = writeIndex.CurrentMessageIndex;
				if (index._currentMessageIndex >= currentMessageIndex)
				{
					data = default(TItem);
					return false;
				}
				LinkedListNode<Ring<TItem>> currentRing = index.currentRing;
				data = currentRing.Value[index.CurrentRingIndex];
				return true;
			}
		}

		/// <summary>
		/// A non-thread safe decrement
		/// </summary>
		/// <param name="toDecrement">The index to decrement</param>
		private void UnsafeDecrement(Index toDecrement)
		{
			int currentRingIndex = toDecrement.CurrentRingIndex;
			currentRingIndex--;
			if (currentRingIndex < 0)
			{
				currentRingIndex = RingSize - 1;
				toDecrement.currentRing = toDecrement.currentRing.Previous ?? chain.Last;
				toDecrement.ringCount--;
			}
			toDecrement.CurrentRingIndex = currentRingIndex;
		}

		private Task GetResizeTask()
		{
			return new Task(delegate
			{
				Ring<TItem>[] array = new Ring<TItem>[RingGrowthIncrement];
				for (int i = 0; i < RingGrowthIncrement; i++)
				{
					array[i] = new Ring<TItem>(RingSize);
				}
				lock (chain)
				{
					int num = writeIndex.ringCount;
					LinkedListNode<Ring<TItem>> node = writeIndex.currentRing;
					for (int j = 0; j < RingGrowthIncrement; j++)
					{
						num++;
						node = chain.AddAfter(node, array[j]);
					}
					HighestInsertedRing = num;
				}
				RingGrowthIncrement *= 2;
				IncrementingChain = null;
			});
		}

		private void IncrementReadIndex(Index toIncrement)
		{
			IncrementIndex(toIncrement, delegate
			{
				UpdateLowestReader();
			});
		}

		private void IncrementWriteIndex(Index toIncrement)
		{
			IncrementIndex(toIncrement, delegate
			{
				if (LowestReadingRing != int.MaxValue && chain.Count - (writeIndex.ringCount - LowestReadingRing) <= 1)
				{
					IncrementingChain = GetResizeTask();
					IncrementingChain.Start();
				}
			});
		}

		/// <summary>
		/// Increments the index to the next available slot
		/// </summary>
		/// <param name="toIncrement"></param>
		/// <param name="chainSafeAction">An action to do while under the chain lock when incremented</param>
		/// <returns>If the ring was updated</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void IncrementIndex(Index toIncrement, Action chainSafeAction)
		{
			int currentRingIndex = toIncrement.CurrentRingIndex;
			currentRingIndex++;
			bool num = Convert.ToBoolean(currentRingIndex / RingSize);
			toIncrement.CurrentRingIndex = currentRingIndex % RingSize;
			Interlocked.Increment(ref toIncrement._currentMessageIndex);
			if (num)
			{
				if (IncrementingChain != null)
				{
					IncrementingChain.Wait();
				}
				toIncrement.ringCount++;
				lock (chain)
				{
					toIncrement.currentRing = toIncrement.currentRing.Next ?? chain.First;
					chainSafeAction();
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void UpdateLowestReader()
		{
			int num = int.MaxValue;
			int i = 0;
			for (int count = readingIndexes.Count; i < count; i++)
			{
				if (num > readingIndexes[i].ringCount)
				{
					num = readingIndexes[i].ringCount;
				}
			}
			LowestReadingRing = num;
		}

		private Index GetLastAvailableReader()
		{
			if (IncrementingChain != null)
			{
				IncrementingChain.Wait();
			}
			lock (chain)
			{
				int highestInsertedRing = HighestInsertedRing;
				int ringCount = writeIndex.ringCount;
				int num = Math.Max(highestInsertedRing, ringCount + 1);
				LinkedListNode<Ring<TItem>> linkedListNode = writeIndex.currentRing;
				int count = chain.Count;
				long currentMessageIndex;
				if (highestInsertedRing != 0 || count <= ringCount)
				{
					int currentRingIndex;
					lock (writeIndex.IndexLock)
					{
						currentRingIndex = writeIndex.CurrentRingIndex;
						currentMessageIndex = writeIndex.CurrentMessageIndex;
					}
					currentMessageIndex -= currentRingIndex;
					int num2 = 0;
					for (int i = ringCount; i < num + 1; i++)
					{
						linkedListNode = linkedListNode.Next ?? chain.First;
						num2++;
					}
					currentMessageIndex -= RingSize * (count - num2);
				}
				else
				{
					for (int num3 = ringCount; num3 != 0; num3--)
					{
						linkedListNode = linkedListNode.Previous;
					}
					currentMessageIndex = 0L;
				}
				Index index = new Index(linkedListNode, 0, currentMessageIndex, num + 1 - count);
				readingIndexes.Add(index);
				UpdateLowestReader();
				return index;
			}
		}

		/// <summary>
		/// Returns a new identifier for a new reader
		/// </summary>
		/// <remarks>
		/// Getting the last data available reader is extemely costly and should be avoided when possible
		/// </remarks>
		/// <param name="options"></param>
		/// <returns>The unique reader</returns>
		public IUniqueReader GetNewReaderIdentifier(IReaderOptions options)
		{
			switch (options)
			{
				case IReaderOptions.LastAvailable:
					return GetLastAvailableReader();
				case IReaderOptions.LastInserted:
					lock (chain)
					{
						Index index;
						lock (writeIndex.IndexLock)
						{
							index = new Index(writeIndex);
						}
						readingIndexes.Add(index);
						UpdateLowestReader();
						return index;
					}
				default:
					throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Copies a reader
		/// </summary>
		/// <param name="copy"></param>
		/// <returns></returns>
		public IUniqueReader GetNewReaderIdentifier(IUniqueReader copy)
		{
			lock (((Index)copy).IndexLock)
			{
				Index index = new Index((Index)copy);
				readingIndexes.Add(index);
				return index;
			}
		}

		/// <summary>
		/// Removes a reader from what's being tracked
		/// </summary>
		/// <param name="toDelete"></param>
		public void RemoveReader(IUniqueReader toDelete)
		{
			Index index = (Index)toDelete;
			lock (index.IndexLock)
			{
				lock (chain)
				{
					readingIndexes.Remove(index);
					UpdateLowestReader();
				}
			}
		}
	}
}
