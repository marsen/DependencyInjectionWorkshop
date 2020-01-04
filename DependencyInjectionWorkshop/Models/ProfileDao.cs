using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Models
{
    public interface IProfile
    {
        /// <summary>
        /// Passwords the specified account identifier.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        string Password(string accountId);
    }

    public class ProfileDao : IProfile
    {
        public ProfileDao()
        {
        }

        /// <summary>
        /// Passwords the specified account identifier.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public string Password(string accountId)
        {
            string passwordFromDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDb = connection.Query<string>("spGetUserPassword", new {Id = accountId},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return passwordFromDb;
        }
    }
}