using System;

namespace Tasks
{
	public class DependencySet : IDependency
	{
		public event Action Completed = delegate {};
		public event Action Scheduled = delegate {};

		public bool IsCompleted { get { return isCompleted; } }
		public bool IsScheduled { get { return isScheduled; } }

		private volatile bool isCompleted;
		private volatile bool isScheduled;
		private readonly IDependency[] dependencies;

		public DependencySet(params IDependency[] dependencies)
		{
			this.dependencies = dependencies;

			isCompleted = true;
			for (int i = 0; i < dependencies.Length; i++)
			{
				isCompleted &= dependencies[i].IsCompleted;
				if(!dependencies[i].IsCompleted)
					dependencies[i].Completed += SingleDependencyComplete;

				if(dependencies[i].IsScheduled)
					isScheduled = true;
				else
					dependencies[i].Scheduled += SingleDependencyScheduled;
			}
		}

		public void Complete()
		{
			for (int i = 0; i < dependencies.Length; i++)
				dependencies[i].Complete();
		}

		private void SingleDependencyComplete()
		{
			bool totalComplete = true;
			for (int i = 0; i < dependencies.Length; i++)
				totalComplete &= dependencies[i].IsCompleted;

			//Check if the entire check is complete, if so: Fire our completed event
			if(totalComplete)
			{
				isCompleted = true;
				Completed();
			}
		}

		private void SingleDependencyScheduled()
		{
			if(!isScheduled)
			{
				isScheduled = true;
				Scheduled();
			}
		}
	}
}