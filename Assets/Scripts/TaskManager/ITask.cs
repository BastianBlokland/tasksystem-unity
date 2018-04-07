namespace Tasks
{
	public interface ITask<InputData, OutputData>
		where InputData : struct, ITaskData
		where OutputData : struct, ITaskData
	{
		OutputData Execute(InputData input);
	}
}