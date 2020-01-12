namespace DependencyInjectionWorkshop.Models
{
    public class LoggerDecorator : AuthenticationDecoratorBase
    {
        private readonly ILogger _logger;
        private readonly IFailedCounter _failedCounter;

        public LoggerDecorator(IAuthenticationService authenticationService, ILogger logger,
            IFailedCounter failedCounter) : base(authenticationService)
        {
            _logger = logger;
            _failedCounter = failedCounter;
        }

        public void Log(string accountId)
        {
            var failedCount = _failedCounter.GetCount(accountId);
            _logger.Log($"accountId:{accountId} failed times:{failedCount}");
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var result = base.Verify(accountId, password, otp);
            if (result == false)
            {
                Log(accountId);
            }

            return result;
        }
    }
}