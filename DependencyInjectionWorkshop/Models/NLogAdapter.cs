namespace DependencyInjectionWorkshop.Models
{
    public interface ILog
    {
        void Info(string accountId, int failedCount);
    }

    public class NLogAdapter : ILog
    {
        public NLogAdapter()
        {
        }

        public void Info(string accountId, int failedCount)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }
    }
}