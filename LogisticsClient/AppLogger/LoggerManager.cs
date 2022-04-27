using log4net;
using log4net.Config;
using System.Reflection;
using System.Xml;

namespace LogisticsClient.AppLogger
{
    public class LoggerManager : ILoggerManager
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(LoggerManager));
        public LoggerManager()
        {
            try
            {
                using (var fs = File.OpenRead("log4net.config"))
                {
                    //Step1 - Prepare xml doc from config file
                    XmlDocument log4xmlDoc = new XmlDocument();
                    log4xmlDoc.Load(fs);

                    //Step2
                    var repo = LogManager.CreateRepository(Assembly.GetEntryAssembly(),
                    typeof(log4net.Repository.Hierarchy.Hierarchy));
                    XmlConfigurator.Configure(repo, log4xmlDoc["log4net"]);
                    _logger.Info("Log System Initialized"); //1st entry
                }
            }
            catch (Exception ex) { _logger.Error("Error", ex); }
        }
        public void LogInformation(string message)
        {
            _logger.Info(message);
        }
    }
}
