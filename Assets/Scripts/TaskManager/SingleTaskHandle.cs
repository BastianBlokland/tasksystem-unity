using System;
using System.Threading;

namespace Tasks
{
	public class SingleTaskHandle<T> : ITaskHandle<T>
		where T : struct, ITaskData
	{
		private T data;
		private readonly ITask<T> task;
		private readonly ActionRunner runner;

		private bool isScheduled;
		private volatile bool isComplete;

		public SingleTaskHandle(T data, ITask<T> task, ActionRunner runner)
		{
			this.data = data;
			this.task = task;
			this.runner = runner;			
		}

		public void Schedule()
		{
			if(isScheduled)
				throw new Exception("[BatchTaskHandle] Allready scheduled");

			runner.Schedule(TreadedExecute);
			isScheduled = true;
		}

		public T Join()
		{
			if(!isScheduled)
				throw new Exception("[BatchTaskHandle] Has not been scheduled yet");

			while(!isComplete)
				runner.Help();
			
			return data;
		}

		//----> RUNNING ON SEPARATE THREAD
		private void TreadedExecute()
		{
			try { task.Execute(ref data); }
			catch(Exception) {} //TODO: Pass this info back to the caller

			isComplete = true;
		}
		//----> RUNNING ON SEPARATE THREAD
	}
}