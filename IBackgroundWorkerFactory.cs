namespace m
{
	public interface IBackgroundWorkerFactory
	{
		IBackgroundWorkerWrapper NewBackgroundWorker();
	}
}