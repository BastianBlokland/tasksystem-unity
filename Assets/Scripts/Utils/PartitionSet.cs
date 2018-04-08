using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Utils
{
	public class PartitionSet<T> : IDisposable
		where T : struct
	{
		private class Entry
		{
			public readonly int Partition;
			public readonly List<T> Data = new List<T>();

			public Entry(int partition)
			{
				Partition = partition;
			}
		}

		private readonly ReaderWriterLockSlim threadLock = new ReaderWriterLockSlim();
		private readonly List<Entry> entries = new List<Entry>();

		public void Add(int partition, T data)
		{
			threadLock.EnterWriteLock();
			{
				Entry entry = Find(partition);
				if(entry == null)
				{
					entry = new Entry(partition);
					entries.Add(entry);
				}
				entry.Data.Add(data);
			}
			threadLock.ExitWriteLock();
		}

		public List<T> Get(int partition)
		{
			Entry entry = null;

			threadLock.EnterReadLock();
			{
				entry = Find(partition);
			}
			threadLock.ExitReadLock();

			return entry != null ? entry.Data : null;
		}

		public void Clear()
		{
			threadLock.EnterWriteLock();
			{
				entries.Clear();
			}
			threadLock.ExitWriteLock();
		}

		public void ClearData()
		{
			threadLock.EnterWriteLock();
			{
				for (int i = 0; i < entries.Count; i++)
					entries[i].Data.Clear();
			}
			threadLock.ExitWriteLock();
		}

		public void Dispose()
		{
			threadLock.Dispose();
		}

		private Entry Find(int partition)
		{
			for (int i = 0; i < entries.Count; i++)
			{
				if(entries[i].Partition == partition)
					return entries[i];
			}
			return null;
		}
	}
}