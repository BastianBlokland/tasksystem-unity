using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Tasks
{
	public class TaskRunner : IDisposable
	{
		//----> Syncing data
		private readonly CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
		private readonly BlockingCollection<TaskActionInfo> actionQueue = new BlockingCollection<TaskActionInfo>();

		public TaskRunner(int executorCount)
		{
			for(int i = 0; i < executorCount; i++)
			{
				Thread executorThread = new Thread(ThreadExecutor);
				executorThread.Priority = ThreadPriority.Highest;
				executorThread.Start();
			}
		}

		public void Schedule(IExecutor executor, int minIndex, int maxIndex, int batch)
		{
			actionQueue.Add(new TaskActionInfo(executor, minIndex, maxIndex, batch));
		}

		public void Help()
		{
			TaskActionInfo action;
			if(actionQueue.TryTake(out action))
			{
				try { action.Execute(); }
				catch(Exception) { }
			}
		}

		public void Dispose()
		{
			cancelTokenSource.Cancel();
		}

		//----> RUNNING ON SEPARATE THREAD
		private void ThreadExecutor()
		{
			while(!cancelTokenSource.IsCancellationRequested)
			{
				try
				{
					TaskActionInfo action = actionQueue.Take(cancelTokenSource.Token);
					try { action.Execute(); }
					catch(Exception) { }
				}
				catch(OperationCanceledException)
				{
					break;
				}
			}
		}
		//----> RUNNING ON SEPARATE THREAD
	}
}