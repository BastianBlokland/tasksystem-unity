using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System;

namespace Tasks
{
	public class TaskManager : IDisposable
	{
		private readonly TaskRunner runner;

		public TaskManager(int executorCount = 7)
		{
			runner = new TaskRunner(executorCount);
		}

		public IDependency ScheduleSingle(ITask task, IDependency dependency = null)
		{
			SingleTaskHandle handle = new SingleTaskHandle(task, runner);
			if(dependency == null || dependency.IsCompleted)
				handle.Schedule(batchSize: 1);
			else
				dependency.Completed += () => handle.Schedule(batchSize: 1);
			return handle;
		}

		public IDependency ScheduleArray<T1>(T1[] data, ITask<T1> task, int batchSize = 10, IDependency dependency = null)
			where T1 : struct
		{
			ArrayTaskHandle<T1> handle = new ArrayTaskHandle<T1>(data, task, runner);	
			if(dependency == null || dependency.IsCompleted)
				handle.Schedule(batchSize);
			else
				dependency.Completed += () => handle.Schedule(batchSize);
			return handle;
		}

		public IDependency ScheduleArray<T1, T2>(T1[] data1, T2[] data2, ITask<T1, T2> task, int batchSize = 10, IDependency dependency = null)
			where T1 : struct
			where T2 : struct
		{
			ArrayTaskHandle<T1, T2> handle = new ArrayTaskHandle<T1, T2>(data1, data2, task, runner);
			if(dependency == null || dependency.IsCompleted)
				handle.Schedule(batchSize);
			else
				dependency.Completed += () => handle.Schedule(batchSize);
			return handle;
		}

		public void Dispose()
		{
			runner.Dispose();
		}
	}
}