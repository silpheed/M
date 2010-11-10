using System;
using GLib;
using Gst;

namespace m
{
	[CoverageExclude("A quick and possibly naive interface to GStreamer-Sharp. Also, I'm lazy.")]
	public class GStreamerAudioStream : IAudioStream
    {
		private MainLoop loop;
	    private PlayBin play;
		private State currentState;
		
		public event LiveUpdateCallback ConstantUpdateEvent;
		
		public GStreamerAudioStream()
		{
			Gst.Application.Init();
	        loop = new MainLoop();
	
	        play = ElementFactory.Make ("playbin", "play") as PlayBin;
	
	        if (play == null) {
	            Console.WriteLine ("error creating a playbin gstreamer object");
	            return;
	        }
		}
		
	    public bool Open(string path)
	    {
	        if (!path.StartsWith("file://"))
				path= "file://" + path;
			
			if (!SetState(State.Null))
				return false;
			play.Uri = path;
			return SetState(State.Ready);
	    }
		
		private bool SetState(State newState)
		{
			if (play == null) {
				currentState = State.Null;
				return false;
			}
			
			play.SetState(newState);
			State state, pending;
			StateChangeReturn sret = play.GetState(out state, out pending, Clock.Second * 5);
			
			if (sret == StateChangeReturn.Async)
				sret = play.GetState (out state, out pending, Clock.Second * 5);
			
			if (sret != StateChangeReturn.Success) {
				Console.WriteLine(String.Format("State change failed for {0} to {1}.", currentState, newState));
				return false;
			}
			
			currentState = newState;
			return true;
		}
		
		public void Play()
		{
			play.Bus.AddWatch(new BusFunc(BusCb));
			SetState(State.Playing);
			loop.Run();
		}
		
		public void PlayPause()
		{
			if (currentState == State.Playing)
				SetState(State.Paused);
			if (currentState == State.Paused)
				SetState(State.Playing);
		}
		
		public void Stop()
		{
			play.SetState(State.Null);
			if (loop.IsRunning)
				loop.Quit();
		}
		
		private bool BusCb(Bus bus, Message message)
	    {
	        switch (message.Type) {
	            case MessageType.Error:
	                string err = String.Empty;
	                message.ParseError (out err);
	                Console.WriteLine ("Gstreamer error: {0}", err);
	                Stop();
	                break;
	            case MessageType.Eos:
	                Stop();
					break;
	        }
	
	        return true;
	    }
		
		~GStreamerAudioStream()
		{
			play = null;
			loop = null;
			
			play.SetState(State.Null);
			Gst.Application.Deinit();
			play.Dispose();
			
		}
	}
}