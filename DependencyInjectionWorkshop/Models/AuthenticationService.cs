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
    public class AuthenticationService
    {
        public bool Verify(string accountId, string password, string otp)
        {
            //// get password
            string passwordFromDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDb = connection.Query<string>("spGetUserPassword", new {Id = accountId},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();

            }
            
            //// get hash
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashedPassword = hash.ToString();

            //// get otp
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            var response = httpClient.PostAsJsonAsync("api/otps", accountId).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }
            var currentOtp = response.Content.ReadAsAsync<string>().Result;
            
            //// compare
            if (currentOtp == otp && hashedPassword == passwordFromDb)
            {
                return true;
            }
            else
            {
                string message = $"account:{accountId} try to login failed";
                var slackClient = new SlackClient("my api token");
                slackClient.PostMessage(messageResponse => { }, "my channel", message, "my bot name");
                return false;
            }
        }

    }
}