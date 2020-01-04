using System;
using System.Net.Http;

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

    public class AuthenticationService : IAuthentication
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IFailedCounter _failedCounter;
        private readonly IOtpService _otpService;
        private readonly ILogger _logger;
        private readonly FailedCounterDecorator _failedCounterDecorator;

        public AuthenticationService(IProfile profile, IHash hash,
            IFailedCounter failedCounter, IOtpService otpService, ILogger logger)
        {
            //_failedCounterDecorator = new FailedCounterDecorator(this);
            _profile = profile;
            _hash = hash;
            _failedCounter = failedCounter;
            _otpService = otpService;
            _logger = logger;
        }

        public AuthenticationService()
        {
            //_failedCounterDecorator = new FailedCounterDecorator(this);
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _failedCounter = new FailedCounter();
            _otpService = new OtpService();
            _logger = new NLoggerAdapter();
        }

        /// <summary>
        /// Verifies the specified account identifier.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="password">The password.</param>
        /// <param name="otp">The otp.</param>
        /// <returns></returns>
        /// <exception cref="DependencyInjectionWorkshop.Models.FailedTooManyTimesException"></exception>
        public bool Verify(string accountId, string password, string otp)
        {
            var isLocked = _failedCounter.IsLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() {AccountId = accountId};
            }

            var passwordFromDb = _profile.Password(accountId);

            var hashedPassword = _hash.ComputeHash(password);

            var currentOtp = _otpService.GetOtp(accountId);

            if (currentOtp == otp && hashedPassword == passwordFromDb)
            {
                //_failedCounterDecorator.Reset(accountId);
                return true;
            }
            else
            {
                _failedCounter.Add(accountId);

                var failedCount = _failedCounter.Get(accountId);
                _logger.Info($"accountId:{accountId} failed times:{failedCount}");
                return false;
            }
        }
    }
}