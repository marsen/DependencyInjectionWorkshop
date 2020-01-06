namespace DependencyInjectionWorkshop.Models
{
    public class NLogger
    {
        public void Log(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
}