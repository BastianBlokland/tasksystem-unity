using System;

namespace Tasks
{
	public class DependencySet : IDependency
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

		private readonly IDependency[] dependencies;

		public DependencySet(params IDependency[] dependencies)
		{
			this.dependencies = dependencies;
			for (int i = 0; i < dependencies.Length; i++)
				dependencies[i].Completed += SingleDependencyComplete;
		}

		public void Complete()
		{
			for (int i = 0; i < dependencies.Length; i++)
				dependencies[i].Complete();
		}

		private void SingleDependencyComplete()
		{
			//Check if the entire check is complete, if so: Fire our completed event
			if(IsComplete)
				Completed();
		}
	}
}