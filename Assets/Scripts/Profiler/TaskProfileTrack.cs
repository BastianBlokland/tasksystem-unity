namespace Profiler
{
	public class TaskProfileTrack : ProfileTrack, Tasks.ITracker
	{
		public void Track(Tasks.IDependency dependency)
		{
			dependency.Scheduled += OnDependencyScheduled;
			dependency.Completed += OnDependencyCompleted;
		}

		private void OnDependencyScheduled()
		{
			LogStartWork();
		}

		private void OnDependencyCompleted()
		{
			LogEndWork();
		}
	}
}