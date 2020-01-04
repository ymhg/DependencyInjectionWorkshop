using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Models
{
    public interface IProfile
    {
        string GetPasswordFromDb(string accountId);
    }

    public class ProfileDao : IProfile
    {
        public ProfileDao()
        {
        }

        /// <summary>
        /// Gets the password from database.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public string GetPasswordFromDb(string accountId)
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