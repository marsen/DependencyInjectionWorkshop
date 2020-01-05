using DependencyInjectionWorkshop.Models.Interface;

namespace DependencyInjectionWorkshop.Models.Adapter
{
    public class NLoggerAdapter : ILogger
    {
        public NLoggerAdapter()
        {
        }

        public void Info(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
}