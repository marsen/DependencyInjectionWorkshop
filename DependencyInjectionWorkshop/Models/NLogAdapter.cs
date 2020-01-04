namespace DependencyInjectionWorkshop.Models
{
    public interface ILogger
    {
        void Info(string accountId, int failedCount);
    }

    public class NLoggerAdapter : ILogger
    {
        public NLoggerAdapter()
        {
        }

        public void Info(string accountId, int failedCount)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }
    }
}