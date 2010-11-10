using System;
using System.Runtime.InteropServices;
using Tao.FFmpeg;
using Tao.OpenAl;

namespace m
{
    public delegate void LiveUpdateCallback(object update);

	[CoverageExclude("Not developed by me.")]
    public class Decoder
    {
        // public static string text;

        private IntPtr pFormatContext;
        private FFmpeg.AVFormatContext formatContext;

        private FFmpeg.AVCodecContext audioCodecContext;
        private IntPtr pAudioCodecContext;
		
        private FFmpeg.AVRational timebase;
        //private IntPtr pAudioStream;

        private IntPtr pAudioCodec;
        //private FFmpeg.AVCodecStruct audioCodec;

        //private readonly String path;
        private int audioStartIndex = -1;
        private int audioSampleRate;
        private int format;
        private const int AUDIO_FRAME_SIZE = 5000;
		private byte[] samples = new byte[AUDIO_FRAME_SIZE];
        private int sampleSize = -1;
        private bool isAudioStream = false;

        private const int TIMESTAMP_BASE = 1000000;

		public event LiveUpdateCallback ConstantUpdateEvent;

        public Decoder()
        {
			FFmpeg.av_register_all();
        }

		~Decoder()
        {
			try {
				if (pFormatContext != IntPtr.Zero)
					FFmpeg.av_close_input_file(pFormatContext);
			}
			catch { }
        }

		public void Dispose()
		{
			try
			{
				if (pFormatContext != IntPtr.Zero)
					FFmpeg.av_close_input_file(pFormatContext);
			}
			catch { }
		}

		public void Reset()
        {
            if (pFormatContext != IntPtr.Zero)
                FFmpeg.av_close_input_file(pFormatContext);
            sampleSize = -1;
            audioStartIndex = -1;
        }

        public bool Open(string path)
        {
            Reset();

            int ret;
            ret = FFmpeg.av_open_input_file(out pFormatContext, path, IntPtr.Zero, 0, IntPtr.Zero);

            if (ret < 0)
            {
                Console.WriteLine("couldn't open input file");
                return false;
            }

            ret = FFmpeg.av_find_stream_info(pFormatContext);

            if (ret < 0)
            {
                Console.WriteLine("couldnt find stream informaion");
                return false;
            }

            formatContext = (FFmpeg.AVFormatContext)
                Marshal.PtrToStructure(pFormatContext, typeof(FFmpeg.AVFormatContext));

            for (int i = 0; i < formatContext.nb_streams; ++i)
            {
                FFmpeg.AVStream stream = (FFmpeg.AVStream)
                       Marshal.PtrToStructure(formatContext.streams[i], typeof(FFmpeg.AVStream));

                FFmpeg.AVCodecContext codec = (FFmpeg.AVCodecContext)
                       Marshal.PtrToStructure(stream.codec, typeof(FFmpeg.AVCodecContext));

                if (codec.codec_type == FFmpeg.CodecType.CODEC_TYPE_AUDIO &&
                                        audioStartIndex == -1)
                {
                    this.pAudioCodecContext = stream.codec;
                    //this.pAudioStream = formatContext.streams[i];
                    this.audioCodecContext = codec;
                    this.audioStartIndex = i;
                    this.timebase = stream.time_base;

                    pAudioCodec = FFmpeg.avcodec_find_decoder(this.audioCodecContext.codec_id);
                    if (pAudioCodec == IntPtr.Zero)
                    {
                        Console.WriteLine("couldn't find codec");
                        return false;
                    }

                    FFmpeg.avcodec_open(stream.codec, pAudioCodec);
                }
            }

            if (audioStartIndex == -1)
            {
                Console.WriteLine("couldn't find audio stream");
                return false;
            }

            audioSampleRate = audioCodecContext.sample_rate;

            if (audioCodecContext.channels == 1)
            {
                format = Al.AL_FORMAT_MONO16;
            }
            else
            {
                format = Al.AL_FORMAT_STEREO16;
            }

            return true;
        }

        static int count = 0;
        public bool Stream()
        {
            int result;

            //  FFmpeg.AVPacket packet = new FFmpeg.AVPacket();
            IntPtr pPacket = Marshal.AllocHGlobal(56);

            //Marshal.StructureToPtr(packet, pPacket, false);
            //  Marshal.PtrToStructure(

            result = FFmpeg.av_read_frame(pFormatContext, pPacket);
            if (result < 0)
                return false;
            count++;

            int frameSize = 0;
            IntPtr pSamples = IntPtr.Zero;
            FFmpeg.AVPacket packet = (FFmpeg.AVPacket)
                                Marshal.PtrToStructure(pPacket, typeof(FFmpeg.AVPacket));
            Marshal.FreeHGlobal(pPacket);

			if (ConstantUpdateEvent != null)
            {
                int cur = (int)(packet.dts * timebase.num / timebase.den);
                int total = (int)(formatContext.duration / TIMESTAMP_BASE);
                string time = String.Format("{0} out of {1} seconds", cur, total);
				ConstantUpdateEvent(time);
            }

            if (packet.stream_index != this.audioStartIndex)
            {
                this.isAudioStream = false;
                return true;
            }
            this.isAudioStream = true;

            try
            {
                pSamples = Marshal.AllocHGlobal(AUDIO_FRAME_SIZE);
                //int size = 
				FFmpeg.avcodec_decode_audio(pAudioCodecContext, pSamples,
                        ref frameSize, packet.data, packet.size);
				//FFmpeg.av_free_packet(pPacket);                                      

                this.sampleSize = frameSize;
                Marshal.Copy(pSamples, samples, 0, AUDIO_FRAME_SIZE);
            }
            catch
            {
                return false;
            }
            finally
            {
                Marshal.FreeHGlobal(pSamples);
            }

            return true;
        }

        public byte[] Samples
        {
            get { return samples; }
        }

        public int SampleSize
        {
            get { return sampleSize; }
        }

        public int Format
        {
            get { return format; }
        }

        public int Frequency
        {
            get { return audioSampleRate; }
        }

        public bool IsAudioStream
        {
            get { return isAudioStream; }
        }

    }
}
