using System.Text;
using DependencyInjectionWorkshop.Models.Interface;

namespace DependencyInjectionWorkshop.Models
{
    public class SHA256Hash : IHash
    {
        public string HashedPassword(string password)
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
}