using System;

namespace Tasks
{
	public class DependencySet : ITaskDependency
	{
		public event Action Completed = delegate {};

		public bool IsComplete 
		{ 
			get
			{
				for (int i = 0; i < dependencies.Length; i++)
					if(!dependencies[i].IsComplete)
						return false;
				return true;
			}
		}

		private readonly ITaskDependency[] dependencies;

		public DependencySet(params ITaskDependency[] dependencies)
		{
			this.dependencies = dependencies;
			for (int i = 0; i < dependencies.Length; i++)
				dependencies[i].Completed += SingleDependencyComplete;
		}

		private void SingleDependencyComplete()
		{
			//Check if the entire check is complete, if so: Fire our completed event
			if(IsComplete)
				Completed();
		}
	}
}