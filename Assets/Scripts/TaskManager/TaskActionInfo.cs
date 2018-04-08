namespace Tasks
{
	public struct TaskActionInfo
	{
		private readonly ITaskExecutor executor;
		private readonly int minIndex;
		private readonly int maxIndex;

		public TaskActionInfo(ITaskExecutor executor, int minIndex, int maxIndex)
		{
			this.executor = executor;
			this.minIndex = minIndex;
			this.maxIndex = maxIndex;
		}

		public void Execute()
		{
			for (int i = minIndex; i <= maxIndex; i++)
				executor.ExecuteElement(i);
		}
	}
}