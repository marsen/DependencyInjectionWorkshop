using System.Text;

namespace DependencyInjectionWorkshop.Models
{
    public interface IHash
    {
        /// <summary>
        /// Hasheds the password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        string ComputeHash(string password);
    }

    public class Sha256Adapter : IHash
    {
        public Sha256Adapter()
        {
        }

        public string ComputeHash(string password)
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
    }
}