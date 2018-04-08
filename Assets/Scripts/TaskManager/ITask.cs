namespace Tasks
{
	public interface ITask<T1>
		where T1 : struct
	{
		void Execute(ref T1 data);
	}

	public interface ITask<T1, T2>
		where T1 : struct
	{
		void Execute(ref T1 data1, ref T2 data2);
	}
}