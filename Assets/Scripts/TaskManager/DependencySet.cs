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

			//Note thread-safety is a bit tricky because if we check the 'IsCompleted' first then subscribe if its not complete yet
			//then the completed event could actually be fired in between those calls. Thats why we first subscribe (even tho it might allready be complete)
			//and then check the bool. In the worst case 'SingleDependencyComplete' gets called twice when the 'Completed' is fired in between the subscribing
			//and the if, but we can safely ignore the second one
			for (int i = 0; i < dependencies.Length; i++)
			{
				dependencies[i].Completed += SingleDependencyComplete;
				dependencies[i].Scheduled += SingleDependencyScheduled;
				
				if(dependencies[i].IsCompleted)
					SingleDependencyComplete();
				if(dependencies[i].IsScheduled)
					SingleDependencyScheduled();
			}
		}

		public void Complete()
		{
			for (int i = 0; i < dependencies.Length; i++)
				dependencies[i].Complete();
		}

		private void SingleDependencyComplete()
		{
			if(isCompleted)
				return;

			//If there is any dependency not complete yet then the set is not complete either
			for (int i = 0; i < dependencies.Length; i++)
			{
				if(!dependencies[i].IsCompleted)
					return;
			}

			isCompleted = true;
			Completed();
		}

		private void SingleDependencyScheduled()
		{
			if(IsScheduled)
				return;
			isScheduled = true;
			Scheduled();
		}
	}
}