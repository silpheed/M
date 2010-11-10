using MbUnit.Framework;

namespace m.tests
{
	[TestFixture]
	public class BackgroundWorkerFactoryFixture : MockingFixture
	{
		private IBackgroundWorkerFactory backgroundWorkerFactory;

		public override void SetUp()
		{
			backgroundWorkerFactory = new BackgroundWorkerFactory();
		}

		[Test]
		public void ObjectCreatedIsBackgroundWorkerWrapper()
		{
			IBackgroundWorkerWrapper bwFromFactory = backgroundWorkerFactory.NewBackgroundWorker();
			
			Assert.IsNotNull(bwFromFactory);
			Assert.AreEqual(typeof(BackgroundWorkerWrapper), bwFromFactory.GetType());
		}
	}
}