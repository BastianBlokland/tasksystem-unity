namespace Tasks
{
	public struct TaskActionInfo
	{
		private readonly ITaskExecutor executor;
		private readonly int index;

		public TaskActionInfo(ITaskExecutor executor, int index)
		{
			this.executor = executor;
			this.index = index;
		}

		public void Execute()
		{
			executor.ExecuteElement(index);
		}
	}
}