using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Utils
{
	public class PartitionSet<T> : IDisposable
		where T : struct
	{
		private readonly ReaderWriterLockSlim threadLock = new ReaderWriterLockSlim();
		private readonly Dictionary<int, List<T>> entries = new Dictionary<int, List<T>>();

		public void Add(int partition, T data)
		{
			threadLock.EnterWriteLock();
			{
				List<T> list;
				if(!entries.TryGetValue(partition, out list))
				{
					list = new List<T>();
					entries.Add(partition, list);
				}
				list.Add(data);
			}
			threadLock.ExitWriteLock();
		}

		public List<T> Get(int partition)
		{
			List<T> result = null;

			threadLock.EnterReadLock();
			{
				entries.TryGetValue(partition, out result);
			}
			threadLock.ExitReadLock();

			return result;
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
				Dictionary<int, List<T>>.Enumerator enumerator = entries.GetEnumerator();
				while(enumerator.MoveNext())
					enumerator.Current.Value.Clear();
			}
			threadLock.ExitWriteLock();
		}

		public void Dispose()
		{
			threadLock.Dispose();
		}
	}
}