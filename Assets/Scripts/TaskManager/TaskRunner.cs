using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Tasks
{
	public class TaskRunner : IDisposable
	{
		//----> Syncing data
		private volatile bool abort;
		private readonly ConcurrentQueue<TaskActionInfo> actionQueue = new ConcurrentQueue<TaskActionInfo>();

		public TaskRunner(int executorCount)
		{
			for(int i = 0; i < executorCount; i++)
				new Thread(ThreadExecutor).Start();
		}

		public void Schedule(IExecutor executor, int minIndex, int maxIndex)
		{
			actionQueue.Enqueue(new TaskActionInfo(executor, minIndex, maxIndex));
		}

		public void Help()
		{
			TaskActionInfo action;
			if(actionQueue.TryDequeue(out action))
			{
				try { action.Execute(); }
				catch(Exception) { }
			}
		}

		public void Dispose()
		{
			abort = true;
		}

		//----> RUNNING ON SEPARATE THREAD
		private void ThreadExecutor()
		{
			while(!abort)
			{
				TaskActionInfo action;
				if(actionQueue.TryDequeue(out action))
				{
					try { action.Execute(); }
					catch(Exception) { }
				}
			}
		}
		//----> RUNNING ON SEPARATE THREAD
	}
}