using System;
using System.Threading;

namespace Tasks
{
	public class BatchTaskHandle<T> : ITaskHandle<T[]>, ITaskExecutor
		where T : struct, ITaskData
	{
		private readonly T[] data;
		private readonly ITask<T> task;
		private readonly TaskRunner runner;

		private bool isScheduled;
		private volatile bool isComplete;
		private int tasksLeft;

		public BatchTaskHandle(T[] data, ITask<T> task, TaskRunner runner)
		{
			this.data = data;
			this.task = task;
			this.runner = runner;			
		}

		public void Schedule()
		{
			if(isScheduled)
				throw new Exception("[BatchTaskHandle] Allready scheduled");

			tasksLeft = data.Length;
			for (int i = 0; i < data.Length; i++)
				runner.Schedule(this, i);
			isScheduled = true;
		}

		public T[] Join()
		{
			if(!isScheduled)
				throw new Exception("[BatchTaskHandle] Has not been scheduled yet");

			while(!isComplete)
				runner.Help();
			
			return data;
		}

		//----> RUNNING ON SEPARATE THREAD
		public void ExecuteElement(int index)
		{
			try { task.Execute(ref data[index]); }
			catch(Exception) { }

			if(Interlocked.Decrement(ref tasksLeft) == 0)
				isComplete = true;
		}
		//----> RUNNING ON SEPARATE THREAD
	}
}