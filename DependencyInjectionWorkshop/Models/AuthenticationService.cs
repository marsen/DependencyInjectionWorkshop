using System;
using System.Net.Http;
using DependencyInjectionWorkshop.Models.Interface;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IProfileInfo _profileInfo = new ProfileInfo();
        private readonly IHash _hash = new SHA256Hash();
        private readonly IOtpService _otpService = new OtpService();
        private readonly IFailedCounter _failedCounter = new FailedCounter();
        private readonly INotify _notifyService = new SlackNotifyService();
        private readonly ILogger _logger = new NLogger();

        
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

                _notifyService.Notify( $"account:{accountId} try to login failed");

                return false;
            }
        }
    }
}