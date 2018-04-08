using System;

namespace Tasks
{
	public interface ITaskHandle
	{
		//NOTE: VERY important to realize that this can be called from any thread
		event Action Completed;

		void Join();
	}
}