namespace DependencyInjectionWorkshop.Models
{
    public interface ILogger
    {
        void Log(string message);
    }

    public class NLogLogger : ILogger
    {
        public NLogLogger()
        {
        }

        public void Log(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
}