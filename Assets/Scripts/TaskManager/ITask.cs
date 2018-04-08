namespace Tasks
{
	public interface ITask<T>
		where T : struct
	{
		void Execute(ref T data);
	}
}