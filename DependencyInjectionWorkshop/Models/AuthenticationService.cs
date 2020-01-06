using System;
using System.Net.Http;
using DependencyInjectionWorkshop.Models.Interface;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IProfileInfo _profileInfo;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;
        private readonly IFailedCounter _failedCounter;
        private readonly INotify _notifyService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationService" /> class.
        /// </summary>
        public AuthenticationService()
        {
            _profileInfo = new ProfileInfo();
            _hash = new SHA256Hash();
            _otpService = new OtpService();
            _failedCounter = new FailedCounter();
            _notifyService = new SlackNotifyService();
            _logger = new NLogger();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };

            _failedCounter.CheckIsLocked(accountId, httpClient);

            var passwordFromDb = _profileInfo.Password(accountId);

            var hashedPassword = _hash.HashedPassword(password);

            var currentOtp = _otpService.CurrentOtp(accountId, httpClient);

            //compare
            if (passwordFromDb == hashedPassword && currentOtp == otp)
            {
                _failedCounter.Reset(accountId, httpClient);

                return true;
            }
            else
            {
                _failedCounter.Add(accountId, httpClient);

                var failedCount = _failedCounter.Get(accountId, httpClient);
                _logger.Log($"accountId:{accountId} failed times:{failedCount}");

                _notifyService.Notify($"account:{accountId} try to login failed");

                return false;
            }
        }
    }
}