// DataBuffers.IBuffer<T>
using System.Collections.Generic;

namespace DataBuffers
{

	/// <summary>
	/// The generic interface for a buffer object
	/// </summary>
	/// <typeparam name="T">The datatype being stored</typeparam>
	public interface IBuffer<T>
	{
		/// <summary>
		/// Gets and sets the size of the buffer
		/// </summary>
		int Size { get; set; }

		/// <summary>
		/// Adds to the buffer
		/// </summary>
		/// <param name="toAdd"></param>
		void Add(T toAdd);

		/// <summary>
		/// Gets the next item for the reader
		/// </summary>
		/// <param name="reader"></param>
		/// <returns>The read item</returns>
		T GetNextItem(IUniqueReader reader);

		/// <summary>
		/// Gets the next item if available, if not returns false
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		bool TryGetNextItem(IUniqueReader reader, out T data);

		/// <summary>
		/// peaks the item if it exists or if not returns false
		/// </summary>
		/// <param name="reader">The reader to act upon</param>
		/// <param name="data"></param>
		/// <returns>If data was set</returns>
		bool TryPeakItem(IUniqueReader reader, out T data);

		/// <summary>
		/// iterates from the current reader position to the most recent inserted item (at that time)
		/// Iterates until all the data has been read or max has been reached. 
		/// </summary>
		/// <param name="reader">reader to act upon</param>
		/// <param name="max">max number of elements read</param>
		/// <returns></returns>
		IEnumerable<T> IterateToCurrentElement(IUniqueReader reader, int max);

		/// <summary>
		/// Creates a new reader
		/// </summary>
		/// <param name="options">Where the reader should start</param>
		/// <returns>The reader</returns>
		IUniqueReader GetNewReaderIdentifier(IReaderOptions options);

		/// <summary>
		/// Copies a reader and it's indexes
		/// </summary>
		/// <param name="copy">The reader to copy the position of</param>
		/// <returns></returns>
		IUniqueReader GetNewReaderIdentifier(IUniqueReader copy);

		/// <summary>
		/// Iterates from the most recent element backwards
		/// </summary>
		/// <param name="max">the maximum amount of elements read</param>
		/// <param name="receivingMessages">If the datastructure will be receiving messages</param>
		/// <returns></returns>
		IEnumerable<T> IterateFromCurrentBackwards(int max, bool receivingMessages);

		/// <summary>
		/// Remove a reader from the internal list of readers
		/// </summary>
		/// <param name="toDelete"></param>
		void RemoveReader(IUniqueReader toDelete);
	}
}