namespace Tasks
{
	public interface ITask<T>
		where T : struct, ITaskData
	{
		void Execute(ref T data);
	}
}