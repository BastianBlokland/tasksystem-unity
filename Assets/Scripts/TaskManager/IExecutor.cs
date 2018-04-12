namespace Tasks
{
	public interface IExecutor
	{
		void ExecuteElement(int index, int batch);
	}
}