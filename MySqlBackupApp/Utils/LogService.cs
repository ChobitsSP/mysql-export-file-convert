using log4net.Repository;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MySqlBackupApp
{
    public static class LogService
    {
        private static ILog logger { get; set; }

        public static void Init(string fileName = "log4net.config")
        {
            var fileInfo = new FileInfo(fileName);
            ILoggerRepository repository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            log4net.Config.XmlConfigurator.Configure(repository, fileInfo);
            logger = LogManager.GetLogger("LogWritter");
        }

        public static ILog GetLogger()
        {
            return logger;
        }

        public static void Init(ILog logger)
        {
            LogService.logger = logger;
        }

        public static void Warn(object message)
        {
            logger.Warn(message);
        }

        public static void Info(string message)
        {
            logger.Info(message);
        }

        public static void Error(string message)
        {
            logger.Error(message);
        }

        public static void Error(Exception exception)
        {
            logger.Error(FlattenException(exception), exception);
        }

        public static string FlattenException(Exception exception)
        {
            var stringBuilder = new StringBuilder();

            while (exception != null)
            {
                stringBuilder.AppendLine(exception.GetType().FullName);
                stringBuilder.AppendLine(exception.Message);
                stringBuilder.AppendLine(exception.StackTrace);

                exception = exception.InnerException;
            }

            return stringBuilder.ToString();
        }
    }
}
