namespace DependencyInjectionWorkshop.Models
{
    public class NLogLogger
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