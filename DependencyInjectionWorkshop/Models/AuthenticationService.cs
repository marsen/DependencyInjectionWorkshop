﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dapper;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public bool Verify(string accountId, string password, string otp)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            
            //check account locked
            var IsLocked = AccountIsLocked(accountId, httpClient);
            if (IsLocked)
            {
                throw new FailedTooManyTimesException(){AccountId = accountId};
            }
            //// get password
            var passwordFromDb = PasswordFromDb(accountId);

            //// get hash
            var hashedPassword = HashedPassword(password);

            //// get otp
            var currentOtp = CurrentOtp(accountId, httpClient);

            //// compare
            if (currentOtp == otp && hashedPassword == passwordFromDb)
            {
                // 失敗次數歸0
                ResetFailedCount(accountId, httpClient);
                return true;
            }
            else
            {

                AddFailedCount(accountId, httpClient);

                LogFailedCount(accountId, httpClient);
                Notify(accountId);
                return false;
            }
        }

        private static void Notify(string accountId)
        {
            //notify
            var message = $"account:{accountId} try to login failed";
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(messageResponse => { }, "my channel", message, "my bot name");
        }

        private static void LogFailedCount(string accountId, HttpClient httpClient)
        {
            // failed log 
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

        private static void ResetFailedCount(string accountId, HttpClient httpClient)
        {
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        private static string CurrentOtp(string accountId, HttpClient httpClient)
        {
            var response = httpClient.PostAsJsonAsync("api/otps", accountId).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }

            var currentOtp = response.Content.ReadAsAsync<string>().Result;
            return currentOtp;
        }

        private static string HashedPassword(string password)
        {
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

        private static string PasswordFromDb(string accountId)
        {
            string passwordFromDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDb = connection.Query<string>("spGetUserPassword", new {Id = accountId},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return passwordFromDb;
        }

        private static bool AccountIsLocked(string accountId, HttpClient httpClient)
        {
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;
            isLockedResponse.EnsureSuccessStatusCode();
            var IsLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return IsLocked;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}