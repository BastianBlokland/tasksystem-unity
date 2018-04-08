namespace Tasks
{
	public interface ITaskHandle<T>
	{
		void Schedule();

		T Join();
	}
}