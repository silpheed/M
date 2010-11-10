using System;
using Rhino.Mocks;
using MbUnit.Framework;
using Rhino.Mocks.Impl;

namespace m.tests
{
	public abstract class MockingFixture
	{
		MockRepository _mocks;

		[SetUp]
		public void BaseSetUp()
		{
			_mocks = new MockRepository();

			//int pos = Debug.Listeners.Add(new TextWriterTraceListener());
			//RhinoMocks.Logger = new TextWriterExpectationLogger(((TextWriterTraceListener)Debug.Listeners[pos]).Writer);
			RhinoMocks.Logger = new TextWriterExpectationLogger(Console.Out);

			SetUp();
		}

		protected MockRepository Mocks
		{
			get {
				return _mocks;
			}
		}

		protected T CreateMock<T>()
		{
			return _mocks.CreateMock<T>();
		}

		protected T DynamicMock<T>()
		{
			return _mocks.DynamicMock<T>();
		}

		protected T PartialMock<T>(params object[] constructorArguments) where T : class
		{
			return _mocks.PartialMock<T>(constructorArguments);
		}

		protected T Stub<T>(params object[] constructorArguments)
		{
			return _mocks.Stub<T>(constructorArguments);
		}

		public abstract void SetUp();

		protected void ReplayAll()
		{
			Mocks.ReplayAll();
		}

		protected void VerifyAll()
		{
			Mocks.VerifyAll();
		}

		protected void ReplaySome(params object[] mockedObjects)
		{
			foreach (object mock in mockedObjects)
				Mocks.Replay(mock);
		}
	}
}

//not in a namespace. it's an ncover thing.
public class CoverageExcludeAttribute : Attribute
{
	private readonly string reason;

	public CoverageExcludeAttribute(string reason)
	{
		this.reason = reason;
	}

	public string Reason
	{
		get { return reason; }
	}
}