using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Profiler
{
	public class ProfileTrack
	{
		private struct Item
		{
			public TimeSpan StartOffset;
			public TimeSpan StopOffset;
			public bool Running;
		}

		private readonly Stopwatch stopWatch = new Stopwatch();
		private readonly ReaderWriterLockSlim threadLock = new ReaderWriterLockSlim();
		private readonly List<Item> items = new List<Item>();
		private bool started;

		public void StartTimer()
		{
			stopWatch.Start();
			started = true;
		}

		public void LogStartWork()
		{
			threadLock.EnterWriteLock();
			{
				if(!started)
					throw new Exception("[ProfileTrack] Unable to log start-work: 'Timer' not yet started");
			
				if(items[items.Count - 1].Running)
					throw new Exception("[ProfileTrack] Unable to log start-work: Last item is still running");

				items.Add(new Item { StartOffset = stopWatch.Elapsed, Running = true });
			}
			threadLock.ExitWriteLock();
		}

		public void LogEndWork()
		{
			threadLock.EnterWriteLock();
			{
				Item lastItem = items[items.Count - 1];
				if(!lastItem.Running)
					throw new Exception("[ProfileTrack] Unable to log end-work: No running item");
				
				lastItem.Running = false;
				lastItem.StopOffset = stopWatch.Elapsed;
				items[items.Count - 1] = lastItem;
			}
			threadLock.ExitWriteLock();
		}
	}
}