namespace Tasks
{
	public interface ITaskHandle<OutputData>
		where OutputData : struct, ITaskData
	{
		bool IsComplete { get; }
		OutputData Data { get; }

		OutputData Join();
	}
}