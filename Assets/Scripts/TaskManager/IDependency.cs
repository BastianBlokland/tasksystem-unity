using System;

namespace Tasks
{
	public interface IDependency
	{
		//NOTE: VERY important to realize that this can be called from any thread
		event Action Completed;

		//NOTE: VERY important to realize that this can be called from any thread
		event Action Scheduled;

		bool IsCompleted { get; }
		bool IsScheduled { get; }

		void Complete();
	}	
}