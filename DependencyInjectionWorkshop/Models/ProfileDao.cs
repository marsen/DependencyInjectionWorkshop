using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Models
{
    public interface IProfile
    {
        /// <summary>
        /// Passwords from database.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        string PasswordFromDb(string accountId);
    }

    public class ProfileDao : IProfile
    {
        public ProfileDao()
        {
        }

        /// <summary>
        /// Passwords from database.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public string PasswordFromDb(string accountId)
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