using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Tasks
{
	public class ActionRunner : IDisposable
	{
		//----> Syncing data
		private volatile bool abort;
		private readonly ConcurrentQueue<Action> actionQueue = new ConcurrentQueue<Action>();

		public ActionRunner(int executorCount = 7)
		{
			for(int i = 0; i < executorCount; i++)
				new Thread(ThreadExecutor).Start();
		}

		public void Schedule(Action action)
		{
			actionQueue.Enqueue(action);
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
				Action action;
				if(actionQueue.TryDequeue(out action))
				{
					try { action(); }
					catch(Exception) { }
				}
				else
					Thread.Sleep(1); //Very short sleep to not 'hog' the cpu
			}
		}
		//----> RUNNING ON SEPARATE THREAD
	}
}