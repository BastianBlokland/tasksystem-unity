using System;
using System.Threading;

namespace Tasks
{
	public abstract class BaseTaskHandle : IDependency, IExecutor
	{
		//NOTE: VERY important to realize that this can be called from any thread
		public event Action Completed = delegate {};
		//NOTE: VERY important to realize that this can be called from any thread
		public event Action Scheduled = delegate {};

		public bool IsCompleted { get { return isCompleted; } }
		public bool IsScheduled { get { return isScheduled; } }

		private readonly int length;
		private readonly TaskRunner runner;

		private volatile bool isCompleted;
		private volatile bool isScheduled;
		private int tasksLeft;

		public BaseTaskHandle(int length, TaskRunner runner)
		{
			this.length = length;
			this.runner = runner;
		}

		public void Schedule(int batchSize)
		{
			if(isScheduled)
				return;

			//Zero or negative batch-sizes are not really supported :)
			if(batchSize <= 0) 
				batchSize = 1;

			//NOTE: Fire the scheduled event BEFORE actually scheduling otherwise a task could be completed before
			//ever firing this event, you gotta love mult-threading :)
			isScheduled = true;
			Scheduled();

			tasksLeft = length;
			int startOffset = batchSize - 1;
			for (int i = 0; i < length; i += batchSize)
			{
				int start = i;
				int end = start + startOffset;
				runner.Schedule(this, start, end >= length ? (length - 1) : end);
			}
		}

		public void Complete()
		{
			while(!isCompleted)
				runner.Help();
		}

		//----> RUNNING ON SEPARATE THREAD
		public void ExecuteElement(int index)
		{
			try { ExecuteTask(index); }
			catch(Exception) { }

			if(Interlocked.Decrement(ref tasksLeft) == 0)
			{
				isCompleted = true;
				Completed();
			}
		}
		//----> RUNNING ON SEPARATE THREAD

		protected abstract void ExecuteTask(int index);
	}
}