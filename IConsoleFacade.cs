namespace m
{
	public interface IConsoleFacade
	{
		void Write(string value);
		void WriteLine(string value);
		string ReadLine();
	}
}