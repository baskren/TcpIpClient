
namespace SequentialCircularBuffers
{
	/// <summary>
	/// A loose rapper around array
	/// </summary>
	/// <typeparam name="TItem"></typeparam>
	internal struct Ring<TItem>
	{
		private readonly TItem[] circBuff;

		public TItem this[int key]
		{
			get
			{
				return circBuff[key];
			}
			set
			{
				circBuff[key] = value;
			}
		}

		public Ring(int size)
		{
			circBuff = new TItem[size];
		}
	}
}