namespace Tasks
{
	public interface ITaskHandle<T>
	{
		T Join();
	}
}