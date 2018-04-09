using System;

namespace Tasks
{
	public interface IDependency
	{
		//NOTE: VERY important to realize that this can be called from any thread
		event Action Completed;

		bool IsComplete { get; }

		void Complete();
	}	
}