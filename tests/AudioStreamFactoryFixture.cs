using MbUnit.Framework;
using Rhino.Mocks;

namespace m.tests
{
	[TestFixture]
	public class AudioStreamFactoryFixture : MockingFixture
	{
		private IAudioStreamFactory audioStreamFactory;
		private IPlatform platform;

		public override void SetUp()
		{
			platform = DynamicMock<IPlatform>();
			audioStreamFactory = new AudioStreamFactory(platform);
		}

		
		private void SetUpWindows()
		{
			SetupResult.For(platform.IsWindows).Return(true);
			SetupResult.For(platform.IsUnix).Return(false);
		}
		
		private void SetUpUnix()
		{
			SetupResult.For(platform.IsWindows).Return(false);
			SetupResult.For(platform.IsUnix).Return(true);
		}
		
		[Test]
		public void CanCreateFFMpegAudioStreamForWindows()
		{
			SetUpWindows();
			ReplayAll();
			
			IAudioStream bwFromFactory = audioStreamFactory.NewAudioStream();
			
			Assert.IsNotNull(bwFromFactory);
			Assert.AreEqual(typeof(FFMpegAudioStream), bwFromFactory.GetType());
			VerifyAll();
		}
		
		[Test]
		public void CanCreateGStreamerAudioStreamForUnix()
		{
			SetUpUnix();
			ReplayAll();
			
			IAudioStream bwFromFactory = audioStreamFactory.NewAudioStream();
			
			Assert.IsNotNull(bwFromFactory);
			Assert.AreEqual(typeof(GStreamerAudioStream), bwFromFactory.GetType());
			VerifyAll();
		}
	}
}