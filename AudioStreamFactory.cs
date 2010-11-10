namespace m
{
	public class AudioStreamFactory : IAudioStreamFactory
	{
		IPlatform platform;
		
		public AudioStreamFactory(IPlatform platform)
		{
			this.platform = platform;
		}
		
		public IAudioStream NewAudioStream()
		{
			if (platform.IsUnix)
			    return new GStreamerAudioStream();
			return new FFMpegAudioStream();
		}
	}
}