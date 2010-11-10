using System.Collections.Generic;

namespace m
{
	public interface ITextDiscriminator
	{
		IList<ICommand> Interpret(string input);
	}
}