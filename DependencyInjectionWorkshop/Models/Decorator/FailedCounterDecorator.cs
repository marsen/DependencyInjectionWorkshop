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
            IsLocked(accountId);
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

        public void IsLocked(string accountId)
        {
            var isLocked = _failedCounter.IsLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() {AccountId = accountId};
            }
        }
    }
}