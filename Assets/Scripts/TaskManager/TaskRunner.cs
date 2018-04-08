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

		public TaskRunner(int executorCount = 7)
		{
			for(int i = 0; i < executorCount; i++)
				new Thread(ThreadExecutor).Start();
		}

		public void Schedule(ITaskExecutor executor, int index)
		{
			actionQueue.Enqueue(new TaskActionInfo(executor, index));
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
				else
					Thread.Sleep(1); //Very short sleep to not 'hog' the cpu
			}
		}
		//----> RUNNING ON SEPARATE THREAD
	}
}