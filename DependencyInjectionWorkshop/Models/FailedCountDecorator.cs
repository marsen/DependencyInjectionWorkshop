namespace DependencyInjectionWorkshop.Models
{
    public class FailedCountDecorator : AuthenticationDecoratorBase
    {
        private readonly IFailedCounter _failedCounter;

        public FailedCountDecorator(IAuthenticationService authenticationService, IFailedCounter failedCounter) : base(
            authenticationService)
        {
            _failedCounter = failedCounter;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            CheckLocked(accountId);
            var result = base.Verify(accountId, password, otp);
            if (result == false)
            {
                Add(accountId);
            }
            else
            {
                Reset(accountId);
            }

            return result;
        }

        public void Add(string accountId)
        {
            _failedCounter.Add(accountId);
        }

        public void Reset(string accountId)
        {
            _failedCounter.Reset(accountId);
        }

        public void CheckLocked(string accountId)
        {
            var isLocked = _failedCounter.IsLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() {AccountId = accountId};
            }
        }
    }
}