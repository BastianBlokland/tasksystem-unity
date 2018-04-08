using System;

namespace Tasks
{
	public interface ITaskHandle : ITaskDependency
	{
		void Join();
	}
}