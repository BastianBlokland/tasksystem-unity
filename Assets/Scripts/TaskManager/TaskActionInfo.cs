namespace Tasks
{
	public struct TaskActionInfo
	{
		private readonly IExecutor executor;
		private readonly int minIndex;
		private readonly int maxIndex;
		private readonly int batch;

		public TaskActionInfo(IExecutor executor, int minIndex, int maxIndex, int batch)
		{
			this.executor = executor;
			this.minIndex = minIndex;
			this.maxIndex = maxIndex;
			this.batch = batch;
		}

		public void Execute()
		{
			for (int i = minIndex; i <= maxIndex; i++)
				executor.ExecuteElement(i, batch);
		}
	}
}