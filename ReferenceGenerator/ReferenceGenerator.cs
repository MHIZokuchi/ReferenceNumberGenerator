using System;
using System.Security.Cryptography;
using System.Text;

namespace ReferenceGenerator
{
    /// <summary>
    /// Provides methods for generating unique reference numbers in various formats.
    /// </summary>
    public class ReferenceGenerator
    {
        private static readonly Random Random = new Random();
        private static readonly string Numbers = "0123456789";
        private static readonly string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly string Alphanumeric = Numbers + Letters;

        /// <summary>
        /// Generates a numeric reference number of specified length.
        /// </summary>
        /// <param name="length">The desired length of the reference number. Must be greater than 0.</param>
        /// <param name="prefix">Optional prefix to add to the reference number.</param>
        /// <returns>A string containing the unique numeric reference number.</returns>
        /// <exception cref="ArgumentException">Thrown when length is less than or equal to 0.</exception>
        public static string GenerateNumeric(int length, string prefix = "")
        {
            if (length <= 0)
                throw new ArgumentException("Length must be greater than 0", nameof(length));

            var result = new StringBuilder(prefix);
            for (var i = 0; i < length; i++)
            {
                result.Append(Numbers[Random.Next(Numbers.Length)]);
            }
            return result.ToString();
        }

        /// <summary>
        /// Generates an alphabetic reference number of specified length.
        /// </summary>
        /// <param name="length">The desired length of the reference number. Must be greater than 0.</param>
        /// <param name="prefix">Optional prefix to add to the reference number.</param>
        /// <returns>A string containing the unique alphabetic reference number.</returns>
        /// <exception cref="ArgumentException">Thrown when length is less than or equal to 0.</exception>
        public static string GenerateAlphabetic(int length, string prefix = "")
        {
            if (length <= 0)
                throw new ArgumentException("Length must be greater than 0", nameof(length));

            var result = new StringBuilder(prefix);
            for (var i = 0; i < length; i++)
            {
                result.Append(Letters[Random.Next(Letters.Length)]);
            }
            return result.ToString();
        }

        /// <summary>
        /// Generates an alphanumeric reference number of specified length.
        /// </summary>
        /// <param name="length">The desired length of the reference number. Must be greater than 0.</param>
        /// <param name="prefix">Optional prefix to add to the reference number.</param>
        /// <returns>A string containing the unique alphanumeric reference number.</returns>
        /// <exception cref="ArgumentException">Thrown when length is less than or equal to 0.</exception>
        public static string GenerateAlphanumeric(int length, string prefix = "")
        {
            if (length <= 0)
                throw new ArgumentException("Length must be greater than 0", nameof(length));

            var result = new StringBuilder(prefix);
            for (var i = 0; i < length; i++)
            {
                result.Append(Alphanumeric[Random.Next(Alphanumeric.Length)]);
            }
            return result.ToString();
        }

        /// <summary>
        /// Generates a cryptographically secure GUID.
        /// </summary>
        /// <returns>A string containing the GUID.</returns>
        public static string GenerateGuid()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Generates a cryptographically secure reference number of specified length.
        /// </summary>
        /// <param name="length">The desired length of the reference number. Must be greater than 0.</param>
        /// <param name="prefix">Optional prefix to add to the reference number.</param>
        /// <returns>A string containing the cryptographically secure reference number.</returns>
        /// <exception cref="ArgumentException">Thrown when length is less than or equal to 0.</exception>
        public static string GenerateSecure(int length, string prefix = "")
        {
            if (length <= 0)
                throw new ArgumentException("Length must be greater than 0", nameof(length));

            var result = new StringBuilder(prefix);
            using (var rng = RandomNumberGenerator.Create())
            {
                var randomBytes = new byte[length];
                rng.GetBytes(randomBytes);

                for (int i = 0; i < length; i++)
                {
                    result.Append(Alphanumeric[randomBytes[i] % Alphanumeric.Length]);
                }
            }
            return result.ToString();
        }
    }
}