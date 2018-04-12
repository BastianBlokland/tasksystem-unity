namespace Tasks
{
	public interface ITask
	{
		void Execute(int index, int batch);
	}

	public interface ITask<T1>
		where T1 : struct
	{
		void Execute(ref T1 data, int index, int batch);
	}

	public interface ITask<T1, T2>
		where T1 : struct
	{
		void Execute(ref T1 data1, ref T2 data2, int index, int batch);
	}
}