namespace DataBuffers
{
	/// <summary>
	/// Indicates various options when a new reader is being made
	/// </summary>
	public enum IReaderOptions
	{
		/// <summary>
		/// Starts reading at the last inserted item
		/// </summary>
		LastInserted,
		/// <summary>
		/// Starts reading at the last available item
		/// </summary>
		LastAvailable
	}
}