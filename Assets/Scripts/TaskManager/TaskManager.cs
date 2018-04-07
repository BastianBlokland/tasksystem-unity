using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System;

namespace Tasks
{
	public class TaskManager : IDisposable
	{
		private class InternalTaskHandle<OutputData> : ITaskHandle<OutputData>
			where OutputData : struct, ITaskData
		{
			//Thread-safe because we're accessing a volatile bool which is garanteed to have a valid value
			public bool IsComplete { get { return isComplete; } }
			
			//Thread-safe because we're checking the 'isComplete' bool and if that is set then the data is guaranteed to be written
			public OutputData Data
			{
				get
				{
					if(!isComplete)
						throw new Exception("[InternalTaskHandle] Unable to get data: Task is not complete!");
					return data;
				}
			}

			/// <summary>
			/// Blocks the thread until the data is available
			/// </summary>
			public OutputData Join()
			{
				while(!isComplete) {}
				return data;
			}

			//----> RUNNING ON SEPARATE THREAD
			private volatile bool isComplete;
			private OutputData data;

			public void ThreadedComplete(OutputData data)
			{
				if(isComplete)
					throw new Exception("[InternalTaskHandle] Task was allready completed!");
				this.data = data;
				isComplete = true;
			}
			//----> RUNNING ON SEPARATE THREAD
		}

		private readonly ActionRunner actionRunner = new ActionRunner();

		public ITaskHandle<OutputData> ScheduleTask<InputData, OutputData>(ITask<InputData, OutputData> task, InputData data)
			where InputData : struct, ITaskData
			where OutputData : struct, ITaskData
		{
			InternalTaskHandle<OutputData> handle = new InternalTaskHandle<OutputData>();
			actionRunner.Schedule(() => ThreadedRun(task, data, handle));
			return handle;
		}

		public void Dispose()
		{
			actionRunner.Dispose();
		}

		//----> RUNNING ON SEPARATE THREAD
		private void ThreadedRun<InputData, OutputData>(ITask<InputData, OutputData> task, InputData data, InternalTaskHandle<OutputData> handle)
			where InputData : struct, ITaskData
			where OutputData : struct, ITaskData
		{
			//Run the task
			OutputData output = task.Execute(data);

			handle.ThreadedComplete(output);
		}
		//----> RUNNING ON SEPARATE THREAD
	}
}