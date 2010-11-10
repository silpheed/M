namespace m
{
	public interface IAudioStream
	{
		bool Open(string path);
		void Play();
		void PlayPause();
		void Stop();
		event LiveUpdateCallback ConstantUpdateEvent;
	}
}