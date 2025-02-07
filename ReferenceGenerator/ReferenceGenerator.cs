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
        private static readonly Random _random = new Random();
        private static readonly string _numbers = "0123456789";
        private static readonly string _letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly string _alphanumeric = _numbers + _letters;

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

            StringBuilder result = new StringBuilder(prefix);
            for (int i = 0; i < length; i++)
            {
                result.Append(_numbers[_random.Next(_numbers.Length)]);
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

            StringBuilder result = new StringBuilder(prefix);
            for (int i = 0; i < length; i++)
            {
                result.Append(_letters[_random.Next(_letters.Length)]);
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

            StringBuilder result = new StringBuilder(prefix);
            for (int i = 0; i < length; i++)
            {
                result.Append(_alphanumeric[_random.Next(_alphanumeric.Length)]);
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

            StringBuilder result = new StringBuilder(prefix);
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] randomBytes = new byte[length];
                rng.GetBytes(randomBytes);

                for (int i = 0; i < length; i++)
                {
                    result.Append(_alphanumeric[randomBytes[i] % _alphanumeric.Length]);
                }
            }
            return result.ToString();
        }
    }
}