using System.Security.Cryptography;
using System.Text;

namespace DependencyInjectionWorkshop.Models
{
    public interface IHash
    {
        string Compute(string password);
    }

    public class Sha256Adapter : IHash
    {
        public Sha256Adapter()
        {
        }

        /// <summary>
        /// Gets the hashed password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public string Compute(string password)
        {
            //hash
            var crypt = new SHA256Managed();
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
}