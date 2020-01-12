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
            var result = base.Verify(accountId, password, otp);
            if (result == false)
            {
                Add(accountId);
            }

            return result;
        }

        public void Add(string accountId)
        {
            _failedCounter.Add(accountId);
        }
    }
}