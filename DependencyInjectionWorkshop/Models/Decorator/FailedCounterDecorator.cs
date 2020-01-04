namespace DependencyInjectionWorkshop.Models.Decorator
{
    public class FailedCounterDecorator : AuthenticationDecoratorBase
    {
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authenticationService, IFailedCounter failedCounter) : base(
            authenticationService)
        {
            _failedCounter = failedCounter;
        }

        private void Reset(string accountId)
        {
            _failedCounter.Reset(accountId);
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var verify = base.Verify(accountId, password, otp);
            if (verify)
            {
                Reset(accountId);
            }
            else
            {
                Add(accountId);
            }

            return verify;
        }

        public void Add(string accountId)
        {
            _failedCounter.Add(accountId);
        }
    }
}