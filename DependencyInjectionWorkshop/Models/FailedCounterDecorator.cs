namespace DependencyInjectionWorkshop.Models
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

    public class AuthenticationDecoratorBase : IAuthentication
    {
        private readonly IAuthentication _authenticationService;

        public AuthenticationDecoratorBase(IAuthentication authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public virtual bool Verify(string accountId, string password, string otp)
        {
            return _authenticationService.Verify(accountId, password, otp);
        }
    }
}