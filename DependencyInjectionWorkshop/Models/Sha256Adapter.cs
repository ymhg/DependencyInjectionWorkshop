using System.Security.Cryptography;
using System.Text;

namespace DependencyInjectionWorkshop.Models
{
    public interface IHash
    {
        string Compute(string inputPassword);
    }

    public class Sha256Adapter : IHash
    {
        public string Compute(string inputPassword)
        {
            var crypt = new SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(inputPassword));
            foreach (var theByte in crypto) hash.Append(theByte.ToString("x2"));

            var hashedPassword = hash.ToString();
            return hashedPassword;
        }
    }
}