using System.Threading;

namespace Utils
{
	/// <summary>
	/// A wrapper around a array and a count, main reason for writing is this that List<T> does a Array.Clear
	/// when you can Clear on it. (some safety for reference types) But because we only use ours on data types
	/// we can make a naive implemention that just sets the count to 0
	/// 
	/// NOTE: There is a exposed 'ReaderWriterLockSlim' so in case you want to play it safe you can use it.
	/// </summary>
	public class SubArray<T>
		where T : struct
	{
		public readonly ReaderWriterLockSlim ThreadLock = new ReaderWriterLockSlim();
		public readonly T[] Data;
		public int Count;

		public SubArray(int capacity)
		{
			Data = new T[capacity];
		}

		public void Add(T data)
		{
			ThreadLock.EnterWriteLock();
			{
				if(Count < Data.Length)
				{
					Data[Count] = data;
					Count++;
				}
			}
			ThreadLock.ExitWriteLock();
		}

		public void Clear()
		{
			ThreadLock.EnterWriteLock();
			{
				Count = 0;
			}
			ThreadLock.ExitWriteLock();
		}
	}
}