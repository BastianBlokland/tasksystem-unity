using System;

namespace Tasks
{
	public class ArrayTaskHandle<T1> : BaseArrayTaskHandle
		where T1 : struct
	{
		private readonly T1[] data;
		private readonly ITask<T1> task;

		public ArrayTaskHandle(T1[] data, ITask<T1> task, TaskRunner runner) 
			: base(data.Length, runner)
		{
			this.data = data;
			this.task = task;
		}

		protected override void ExecuteTask(int index)
		{
			task.Execute(ref data[index]);
		}
	}

	public class ArrayTaskHandle<T1, T2> : BaseArrayTaskHandle
		where T1 : struct
		where T2 : struct
	{
		private readonly T1[] data1;
		private readonly T2[] data2;
		private readonly ITask<T1, T2> task;

		public ArrayTaskHandle(T1[] data1, T2[] data2, ITask<T1, T2> task, TaskRunner runner) 
			: base(data1.Length, runner)
		{
			this.data1 = data1;
			this.data2 = data2;
			this.task = task;

			if(data1.Length != data2.Length)
				throw new Exception("[ArrayTaskHandle] Both input arrays MUST have the same length");
		}

		protected override void ExecuteTask(int index)
		{
			task.Execute(ref data1[index], ref data2[index]);
		}
	}
}