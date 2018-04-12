using System;

namespace Tasks
{
	public class SingleTaskHandle : BaseTaskHandle
	{
		private readonly ITask task;

		public SingleTaskHandle(ITask task, TaskRunner runner)
			: base(1, runner)
		{
			this.task = task;
		}

		protected override void ExecuteTask(int index, int batch)
		{
			task.Execute(index, batch);
		}
	}
}