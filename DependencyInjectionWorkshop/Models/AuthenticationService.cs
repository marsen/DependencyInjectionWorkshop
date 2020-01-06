using System;
using System.Net.Http;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class OtpService
    {
        public string CurrentOtp(string accountId, HttpClient httpClient)
        {
            //get otp
            var response = httpClient.PostAsJsonAsync("api/otps", accountId).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }

            var currentOtp = response.Content.ReadAsAsync<string>().Result;
            return currentOtp;
        }
    }

    public class FailedCounter
    {
        public void ResetFailedCount(string accountId, HttpClient httpClient)
        {
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        public void CheckIsLocked(string accountId, HttpClient httpClient)
        {
            //check account locked
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            if (isLockedResponse.Content.ReadAsAsync<bool>().Result)
            {
                throw new FailedTooManyTimesException() { AccountId = accountId };
            }
        }

        public void AddFailedCount(string accountId, HttpClient httpClient)
        {
            //失敗
            var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        public int GetFailedCount(string accountId, HttpClient httpClient)
        {
            //紀錄失敗次數
            var failedCountResponse =
                httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }
    }

    public class SlackNotifyService
    {
        public void Notify(string accountId)
        {
            //notify
            string message = $"account:{accountId} try to login failed";
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(messageResponse => { }, "my channel", message, "my bot name");
        }
    }

    public class NLogger
    {
        public void Log(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }

    public class AuthenticationService
    {
        private readonly ProfileInfo _profileInfo = new ProfileInfo();
        private readonly SHA256Hash _sha256Hash = new SHA256Hash();
        private readonly OtpService _otpService = new OtpService();
        private readonly FailedCounter _failedCounter = new FailedCounter();
        private readonly SlackNotifyService _slackNotifyService = new SlackNotifyService();
        private readonly NLogger _nLogger = new NLogger();

        public bool Verify(string accountId, string password, string otp)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };

            _failedCounter.CheckIsLocked(accountId, httpClient);

            var passwordFromDb = _profileInfo.GetPasswordFromDb(accountId);

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

                _slackNotifyService.Notify(accountId);

                return false;
            }
        }
    }
}