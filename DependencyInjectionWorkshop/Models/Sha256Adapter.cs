using System.Text;

namespace DependencyInjectionWorkshop.Models
{
    public class Sha256Adapter : IHash
    {
        public Sha256Adapter()
        {
        }

        /// <summary>
        /// Computes the hash.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns></returns>
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