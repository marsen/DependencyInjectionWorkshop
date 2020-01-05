namespace DependencyInjectionWorkshop.Models
{
    public interface IHash
    {
        /// <summary>
        /// Computes the hash.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        string ComputeHash(string password);
    }
}