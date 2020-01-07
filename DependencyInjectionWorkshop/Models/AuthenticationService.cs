using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly SlackNotify _slackNotify = new SlackNotify();
        private readonly Profile _profile = new Profile();
        private readonly Sha256Hash _sha256Hash = new Sha256Hash();
        private readonly OtpService _otpService = new OtpService();
        private readonly FailedCounter _failedCounter = new FailedCounter();

        public bool Verify(string accountId, string password, string otp)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };

            _failedCounter.CheckIsLocked(accountId, httpClient);

            var passwordFromDb = _profile.GetPassword(accountId);

            var hashedPassword = _sha256Hash.HashPassword(password);

            var currentOtp = _otpService.CurrentOtp(accountId, httpClient);

            //compare
            if (passwordFromDb == hashedPassword && currentOtp == otp)
            {
                _failedCounter.ResetFailedCount(accountId, httpClient);

                return true;
            }
            else
            {
                _failedCounter.AddFailedCount(accountId, httpClient);

                _failedCounter.LogFailedCount(accountId, httpClient);

                _slackNotify.Notify(accountId);

                return false;
            }
        }
    }
}