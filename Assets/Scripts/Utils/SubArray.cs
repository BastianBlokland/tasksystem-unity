namespace Utils
{
	/// <summary>
	/// A wrapper around a array and a count, main reason for writing is this that List<T> does a Array.Clear
	/// when you can Clear on it. (some safety for reference types) But because we only use ours on data types
	/// we can make a naive implemention that just sets the count to 0
	/// 
	/// NOTE: This class gives no safety against reading or writing beyond the 'Count' so use with care :)
	/// </summary>
	public class SubArray<T>
		where T : struct
	{
		public readonly T[] Data;
		public int Count;

		public SubArray(int capacity)
		{
			Data = new T[capacity];
		}

		public void Add(T data)
		{
			if(Count < Data.Length)
			{
				Data[Count] = data;
				Count++;
			}
		}

		public void Clear()
		{
			Count = 0;
		}
	}
}