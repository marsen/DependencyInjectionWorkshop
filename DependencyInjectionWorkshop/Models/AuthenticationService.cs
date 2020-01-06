﻿using System;
using System.Net.Http;
using DependencyInjectionWorkshop.Models.Interface;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IProfileInfo _profileInfo = new ProfileInfo();
        private readonly SHA256Hash _sha256Hash = new SHA256Hash();
        private readonly OtpService _otpService = new OtpService();
        private readonly FailedCounter _failedCounter = new FailedCounter();
        private readonly SlackNotifyService _slackNotifyService = new SlackNotifyService();
        private readonly NLogger _nLogger = new NLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationService" /> class.
        /// </summary>
        public AuthenticationService()
        {
            _profileInfo = new ProfileInfo();
        }
        public bool Verify(string accountId, string password, string otp)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };

            _failedCounter.CheckIsLocked(accountId, httpClient);

            var passwordFromDb = _profileInfo.Password(accountId);

            var hashedPassword = _sha256Hash.HashedPassword(password);

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

                var failedCount = _failedCounter.GetFailedCount(accountId, httpClient);
                _nLogger.Log($"accountId:{accountId} failed times:{failedCount}");

                _slackNotifyService.Notify( $"account:{accountId} try to login failed");

                return false;
            }
        }
    }
}