using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Profiler
{
	public class ProfileTrack
	{
		private readonly Stopwatch stopWatch = new Stopwatch();
		private readonly ReaderWriterLockSlim threadLock = new ReaderWriterLockSlim();
		private readonly List<TrackItem> items = new List<TrackItem>();
		private bool started;

		public void StartTimer()
		{
			stopWatch.Start();
			started = true;
		}

		public void GetItems(List<TrackItem> outputList)
		{
			outputList.Clear();
			threadLock.EnterReadLock();
			{
				for (int i = 0; i < items.Count; i++)
					outputList.Add(items[i]);
			}
			threadLock.ExitReadLock();
		}

		public void LogStartWork()
		{
			threadLock.EnterWriteLock();
			{
				if(!started)
					throw new Exception("[ProfileTrack] Unable to log start-work: 'Timer' not yet started");
			
				if(items.Count > 0 && items[items.Count - 1].Running)
					throw new Exception("[ProfileTrack] Unable to log start-work: Last item is still running");

				items.Add(new TrackItem { StartTime = (float)stopWatch.Elapsed.TotalSeconds, Running = true });
			}
			threadLock.ExitWriteLock();
		}

		public void LogEndWork()
		{
			threadLock.EnterWriteLock();
			{
				if(items.Count == 0)
					throw new Exception("[ProfileTrack] Unable to log end-work: No item started yet");

				TrackItem lastItem = items[items.Count - 1];
				if(!lastItem.Running)
					throw new Exception("[ProfileTrack] Unable to log end-work: No running item");
				
				lastItem.Running = false;
				lastItem.StopTime = (float)stopWatch.Elapsed.TotalSeconds;
				items[items.Count - 1] = lastItem;
			}
			threadLock.ExitWriteLock();
		}
	}
}