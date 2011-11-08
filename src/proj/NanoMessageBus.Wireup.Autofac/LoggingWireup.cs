namespace NanoMessageBus
{
	using Logging;

	public class LoggingWireup : WireupModule
	{
		public LoggingWireup(IWireup wireup)
			: base(wireup)
		{
		}

		public virtual LoggingWireup UseConsoleWindow()
		{
			ConsoleWindowLogger.MakePrimaryLogger();
			return this;
		}
		public virtual LoggingWireup UseOutputWindow()
		{
			OutputWindowLogger.MakePrimaryLogger();
			return this;
		}
		public virtual LoggingWireup UseNLog()
		{
			NLogLogger.MakePrimaryLogger();
			return this;
		}
		public virtual LoggingWireup UseLog4Net()
		{
			Log4NetLogger.MakePrimaryLogger();
			return this;
		}
	}
}