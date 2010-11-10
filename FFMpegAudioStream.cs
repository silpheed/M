using System;
using Tao.OpenAl;
using System.Threading;
using System.Diagnostics;

namespace m
{
	[CoverageExclude("Mostly not developed by me.")]
	public class FFMpegAudioStream : IAudioStream
    {
		//4 seems to be as low as the number of buffers can go without sounding like crap.
		//The higher the number of buffers, the more ffmpeg will cache from the file.
		//Why is this bad? The greater the number of buffers, the longer the delay in registering external input. e.g. pausing
		//private const int MAX_BUFFERS = 10;
		private const int MAX_BUFFERS = 4;
        private Decoder decoder;
        private int source;
        private int[] buffers = new int[MAX_BUFFERS];
        private float[] zeros = { 0.0f, 0.0f, 0.0f };
        private bool playing;

		public bool paused = false;

        public event LiveUpdateCallback ConstantUpdateEvent;

        public FFMpegAudioStream()
        {
            decoder = new Decoder();
			decoder.ConstantUpdateEvent += new LiveUpdateCallback(decoder_ConstantUpdateEvent);

            Alut.alutInit();
            Al.alGenBuffers(MAX_BUFFERS, buffers);
            Check();

            Al.alGenSources(1,out source);
            Check();

            Al.alSourcefv(source, Al.AL_POSITION, zeros);
            Al.alSourcefv(source, Al.AL_VELOCITY, zeros);
            Al.alSourcefv(source, Al.AL_DIRECTION, zeros);
            Al.alSourcef(source, Al.AL_ROLLOFF_FACTOR, 0.0f);
            Al.alSourcei(source, Al.AL_SOURCE_RELATIVE, Al.AL_TRUE);
        }



		void decoder_ConstantUpdateEvent(object update)
        {
			if (ConstantUpdateEvent != null)
				ConstantUpdateEvent(update);
        }

        public bool Open(string path)
        {
			return decoder.Open(path);
        }

        public void Play()
        {
            PlayFunc();
			Stop();
        }

		public void PlayPause()
		{
			paused = paused == false;
		}

		private bool Update()
        {
            int processed;
            bool active = true;

            Al.alGetSourcei(source, Al.AL_BUFFERS_PROCESSED, out processed);

            while (processed-- > 0) {
				int buffer = -1;

                Al.alSourceUnqueueBuffers(source, 1, ref buffer);
                Check();

                active = Stream(buffer);

                if (active) {
                    Al.alSourceQueueBuffers(source, 1, ref buffer);
                    Check();
                }
            }

            return active;
        }

        private void Check()
        {
            int error = Al.alGetError();
            if (error != Al.AL_NO_ERROR) {
                Debug.WriteLine("OpenAL error: " + Al.alGetString(error));
            }

			while (paused)
			{
				Thread.Sleep(10);
				continue;
			}

        }

        private bool Stream(int buffer)
        {
            if (decoder.Stream()) {
                if (decoder.IsAudioStream) {
                    byte[] samples = decoder.Samples;
                    int sampleSize = decoder.SampleSize;
					Al.alBufferData(buffer, decoder.Format, samples, sampleSize, decoder.Frequency);
                    Check();
                }
                return true;
            }
            return false;
        }

        public bool IsPlaying
        {
			get {
	            int state;
	            Al.alGetSourcei(source, Al.AL_SOURCE_STATE, out state);
	            return (state == Al.AL_PLAYING);
			}
        }

        public void Stop()
        {
            playing = false;
        	paused = false;
            Al.alSourceStop(source);
			//decoder.Reset();
		}

        public bool Playback()
        {
            int queue = 0;
            Al.alGetSourcei(source, Al.AL_BUFFERS_QUEUED, out queue);

            if (queue > 0) {
                Al.alSourcePlay(source);
            }

            if(IsPlaying)
                return true;

            for(int i = 0; i < MAX_BUFFERS; ++i)
            {
                if (!Stream(buffers[i]))
                    return false;
            }

            Al.alSourceQueueBuffers(source, MAX_BUFFERS, buffers);            

            return true;
        }

        private void PlayFunc()
        {
            playing = true;
        	paused = false;
            if (!Playback())
                throw new Exception("Refused to play.");
            while (Update()) {
                if (!playing)
                    break;
				
				Thread.Sleep(10);
            	if (!IsPlaying) {
                    Playback();
                }
            }
        }

    	public bool IsPaused
    	{
			get { return paused; }
    	}
    }
}
