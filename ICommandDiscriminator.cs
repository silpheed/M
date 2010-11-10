using System.Collections.Generic;

namespace m
{
	public interface ICommandDiscriminator
	{
		bool Process(IList<ICommand> commandList);
	}
}