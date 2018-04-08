using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System;

namespace Tasks
{
	public class TaskManager : IDisposable
	{
		private readonly TaskRunner runner = new TaskRunner();

		public ITaskHandle ScheduleArray<T1>(T1[] data, ITask<T1> task, int batchSize = 10)
			where T1 : struct
		{
			ArrayTaskHandle<T1> handle = new ArrayTaskHandle<T1>(data, task, runner);
			handle.Schedule(batchSize);
			return handle;
		}

		public ITaskHandle ScheduleArray<T1, T2>(T1[] data1, T2[] data2, ITask<T1, T2> task, int batchSize = 10)
			where T1 : struct
			where T2 : struct
		{
			ArrayTaskHandle<T1, T2> handle = new ArrayTaskHandle<T1, T2>(data1, data2, task, runner);
			handle.Schedule(batchSize);
			return handle;
		}

		public void Dispose()
		{
			runner.Dispose();
		}
	}
}