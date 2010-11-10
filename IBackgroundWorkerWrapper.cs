using System;
using System.ComponentModel;
using System.Runtime.Remoting;

namespace m
{
	public interface IBackgroundWorkerWrapper
	{
		event DoWorkEventHandler DoWork;
		event ProgressChangedEventHandler ProgressChanged;
		event RunWorkerCompletedEventHandler RunWorkerCompleted;
		void CancelAsync();
		void ReportProgress(int percentProgress);
		void ReportProgress(int percentProgress, object userState);
		void RunWorkerAsync();
		void RunWorkerAsync(object argument);

		bool CancellationPending { get; }

		bool IsBusy { get; }

		bool WorkerReportsProgress { get; set; }

		bool WorkerSupportsCancellation { get; set; }

		event EventHandler Disposed;

		void Dispose();
		string ToString();

		ISite Site { get; set; }

		IContainer Container { get; }
		/*
		object GetLifetimeService();
		object InitializeLifetimeService();
		ObjRef CreateObjRef(Type requestedType);
		*/
	}
}