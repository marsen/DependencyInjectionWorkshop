using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dapper;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class SlackNotify
    {
        public void Notify(string accountId)
        {
            //notify
            string message = $"account:{accountId} try to login failed";
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(messageResponse => { }, "my channel", message, "my bot name");
        }
    }

    public class Profile
    {
        public string GetPassword(string accountId)
        {
            //get password
            string passwordFromDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDb = connection.Query<string>("spGetUserPassword", new {Id = accountId},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return passwordFromDb;
        }
    }

    public class SHA256Hash
    {
        public string HashPassword(string password)
        {
            //hash
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashedPassword = hash.ToString();
            return hashedPassword;
        }
    }

    public class AuthenticationService
    {
        private readonly SlackNotify _slackNotify = new SlackNotify();
        private readonly Profile _profile = new Profile();
        private readonly SHA256Hash _sha256Hash = new SHA256Hash();

        public bool Verify(string accountId, string password, string otp)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };

            CheckIsLocked(accountId, httpClient);

            var passwordFromDb = _profile.GetPassword(accountId);

            var hashedPassword = _sha256Hash.HashPassword(password);

            var currentOtp = CurrentOtp(accountId, httpClient);

            //compare
            if (passwordFromDb == hashedPassword && currentOtp == otp)
            {
                var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
                resetResponse.EnsureSuccessStatusCode();

                return true;
            }
            else
            {
                AddFailedCount(accountId, httpClient);

                LogFailedCount(accountId, httpClient);

                _slackNotify.Notify(accountId);

                return false;
            }
        }

        private  void LogFailedCount(string accountId, HttpClient httpClient)
        {
            //紀錄失敗次數 
            var failedCountResponse =
                httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }

        private static void AddFailedCount(string accountId, HttpClient httpClient)
        {
            //失敗
            var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        private static string CurrentOtp(string accountId, HttpClient httpClient)
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

        private  void CheckIsLocked(string accountId, HttpClient httpClient)
        {
            //check account locked
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            if (isLockedResponse.Content.ReadAsAsync<bool>().Result)
            {
                throw new FailedTooManyTimesException() {AccountId = accountId};
            }
        }
    }
}