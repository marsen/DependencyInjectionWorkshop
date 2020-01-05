namespace DependencyInjectionWorkshop.Models.Adapter
{
    public interface ILogger
    {
        void Info(string message);
    }

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