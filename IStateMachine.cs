using System.Collections.Generic;

namespace m
{
	//IStateMachine is not named after its implementation (i.e. it isn't named IPlayer) because it could be a true interface to other 
	//implementations, for example a lyrics player or a streaming server.

	public interface IStateMachine
	{
		void ProcessCommand(ICommand command);
		bool IsStopped { get; }
		string CurrentTrack { get; }
		int CurrentTrackOrdinal { get; }
		bool IsPlaying { get; }
		bool IsPaused { get; }
		LoopType CurrentLoopType { get; }
		IList<string> History { get; }
	}
}