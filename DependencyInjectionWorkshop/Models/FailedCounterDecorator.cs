namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator:IAuthentication
    {
        private readonly IAuthentication _authenticationService;
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authenticationService,IFailedCounter failedCounter)
        {
            _authenticationService = authenticationService;
            _failedCounter = failedCounter;
        }

        private void Reset(string accountId)
        {
            _failedCounter.Reset(accountId);
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var verify = this._authenticationService.Verify(accountId,password,otp);
            if (verify)
            {
                Reset(accountId);
            }

            return verify;
        }
    }
}