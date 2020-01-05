using DependencyInjectionWorkshop.Models.Adapter;
using DependencyInjectionWorkshop.Models.Decorator;
using DependencyInjectionWorkshop.Models.Interface;

namespace DependencyInjectionWorkshop.Models
{
    public class LoggerDecorator : AuthenticationDecoratorBase
    {
        private readonly ILogger _logger;
        private readonly IFailedCounter _failedCounter;

        public LoggerDecorator(IAuthentication authenticationService, ILogger logger, IFailedCounter failedCounter) :
            base(authenticationService)
        {
            _logger = logger;
            _failedCounter = failedCounter;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var verify = base.Verify(accountId, password, otp);
            if (verify == false)
            {
                Log(accountId);
            }

            return verify;
        }

        private void Log(string accountId)
        {
            var failedCount = _failedCounter.Get(accountId);
            _logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }
    }
}