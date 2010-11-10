namespace m
{
	public class BackgroundWorkerFactory : IBackgroundWorkerFactory
	{
		public IBackgroundWorkerWrapper NewBackgroundWorker()
		{
			return new BackgroundWorkerWrapper();
		}
	}
}