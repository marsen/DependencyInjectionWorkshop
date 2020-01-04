using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService : IAuthentication
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly INotification _notification;
        private readonly IFailedCounter _failedCounter;
        private readonly IOtpService _otpService;
        private readonly ILogger _logger;

        public AuthenticationService(IProfile profile, IHash hash, INotification notification,
            IFailedCounter failedCounter, IOtpService otpService, ILogger logger)
        {
            _profile = profile;
            _hash = hash;
            _notification = notification;
            _failedCounter = failedCounter;
            _otpService = otpService;
            _logger = logger;
        }

        public AuthenticationService()
        {
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _notification = new SlackAdapter();
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
                _failedCounter.Reset(accountId);
                return true;
            }
            else
            {
                _failedCounter.Add(accountId);

                var failedCount = _failedCounter.Get(accountId);
                _logger.Info($"accountId:{accountId} failed times:{failedCount}");
                Notify(accountId);
                return false;
            }
        }

        private void Notify(string accountId)
        {
            _notification.Notify(accountId, $"account:{accountId} try to login failed");
        }
    }
}