using System;
using log4net;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// Log4Net提供的日志记录器
    /// </summary>
    public class Log4NetLogger : ILogger
    {
        private ILog _log;

        public Log4NetLogger(ILog log)
        {
            this._log = log;
        }

        #region ILogger Members

        public void Info(object message)
        {
            this._log.Info(message);
        }

        public void InfoFormat(string format, params object[] args)
        {
            this._log.InfoFormat(format, args);
        }

        public void Info(object message, Exception exception)
        {
            this._log.Info(message, exception);
        }

        public void Error(object message)
        {
            this._log.Error(message);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            this._log.ErrorFormat(format, args);
        }

        public void Error(object message, Exception exception)
        {
            this._log.Error(message, exception);
        }

        public void Warn(object message)
        {
            this._log.Warn(message);
        }

        public void WarnFormat(string format, params object[] args)
        {
            this._log.WarnFormat(format, args);
        }

        public void Warn(object message, Exception exception)
        {
            this._log.Warn(message, exception);
        }

        public bool IsDebugEnabled
        {
            get
            {
                return _log.IsDebugEnabled;
            }
        }

        public void Debug(object message)
        {
            this._log.Debug(message);
        }

        public void DebugFormat(string format, params object[] args)
        {
            this._log.DebugFormat(format, args);
        }

        public void Debug(object message, Exception exception)
        {
            this._log.Debug(message, exception);
        }

        public void Fatal(object message)
        {
            this._log.Fatal(message);
        }

        public void FatalFormat(string format, params object[] args)
        {
            this._log.FatalFormat(format, args);
        }

        public void Fatal(object message, Exception exception)
        {
            this._log.Fatal(message, exception);
        }

        #endregion
    }
}