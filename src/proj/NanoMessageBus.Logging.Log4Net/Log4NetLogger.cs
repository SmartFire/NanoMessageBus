namespace NanoMessageBus.Logging
{
	using System;
	using log4net;

	public class Log4NetLogger : ILog
	{
		public static void MakePrimaryLogger()
		{
			LogFactory.BuildLogger = type => new Log4NetLogger(type);
		}

		private readonly log4net.ILog log;

		public Log4NetLogger(Type typeToLog)
		{
			this.log = LogManager.GetLogger(typeToLog);
		}

		public virtual void Verbose(string message, params object[] values)
		{
			if (this.log.IsDebugEnabled)
				this.log.DebugFormat(message, values);
		}
		public virtual void Debug(string message, params object[] values)
		{
			if (this.log.IsDebugEnabled)
				this.log.DebugFormat(message, values);
		}
		public virtual void Info(string message, params object[] values)
		{
			if (this.log.IsInfoEnabled)
				this.log.InfoFormat(message, values);
		}
		public virtual void Warn(string message, params object[] values)
		{
			if (this.log.IsWarnEnabled)
				this.log.WarnFormat(message, values);
		}
		public virtual void Error(string message, params object[] values)
		{
			if (this.log.IsErrorEnabled)
				this.log.ErrorFormat(message, values);
		}
		public virtual void Fatal(string message, params object[] values)
		{
			if (this.log.IsFatalEnabled)
				this.log.FatalFormat(message, values);
		}
	}
}