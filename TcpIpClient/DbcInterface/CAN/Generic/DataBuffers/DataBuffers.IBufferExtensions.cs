// DataBuffers.IBufferExtensions
using System.Collections.Generic;

namespace DataBuffers
{

	/// <summary>
	/// Various extension methods for an IBuffer
	/// </summary>
	public static class IBufferExtensions
	{
		/// <summary>
		/// Copies every element in the IBuffer to the list
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="buffer"></param>
		/// <returns></returns>
		public static List<T> ToList<T>(this IBuffer<T> buffer)
		{
			List<T> list = new List<T>();
			foreach (T item in buffer.IterateFromCurrentBackwards(int.MaxValue, receivingMessages: false))
			{
				list.Add(item);
			}
			return list;
		}

		/// <summary>
		/// Fills the buffer with all the elements in the array
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="elements"></param>
		public static void Fill<T>(this IBuffer<T> buffer, List<T> elements)
		{
			foreach (T element in elements)
			{
				buffer.Add(element);
			}
		}
	}
}