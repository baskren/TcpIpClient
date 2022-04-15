// DataBuffers.CircularBuffer<TItem>
using System;
using System.Collections.Generic;

namespace DataBuffers
{

	/// <summary>
	/// This structure contains a non-thread safe circular buffer
	/// </summary>
	/// <typeparam name="TItem"></typeparam>
	internal class CircularBuffer<TItem> : IBuffer<TItem>
	{
		private TItem[] buffer;

		public int Size
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public CircularBuffer(int size)
		{
			buffer = new TItem[size];
		}

		public void Add(TItem toAdd)
		{
			throw new NotImplementedException();
		}

		public IUniqueReader GetNewReaderIdentifier(IReaderOptions options)
		{
			throw new NotImplementedException();
		}

		public IUniqueReader GetNewReaderIdentifier(IUniqueReader copy)
		{
			throw new NotImplementedException();
		}

		public void RemoveReader(IUniqueReader toDelete)
		{
			throw new NotImplementedException();
		}

		public TItem GetNextItem(IUniqueReader reader)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<TItem> IterateFromCurrentBackwards(int max, bool receivingMessages)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<TItem> IterateToCurrentElement(IUniqueReader reader, int max)
		{
			throw new NotImplementedException();
		}

		public bool TryGetNextItem(IUniqueReader reader, out TItem data)
		{
			throw new NotImplementedException();
		}

		public bool TryPeakItem(IUniqueReader reader, out TItem data)
		{
			throw new NotImplementedException();
		}
	}
}
