using System;
using System.Security.Cryptography;
using System.Text;
using ReferenceGenerator.CheckDigit;
using ReferenceGenerator.Templates;

namespace ReferenceGenerator;

/// <summary>
/// A lightweight, flexible library for generating unique reference numbers in various formats.
/// Perfect for creating order numbers, transaction IDs, document references, and more.
/// </summary>
public class ReferenceNumberGenerator : IReferenceGenerator
{
    private static readonly Random SharedRandom = new();
    private static readonly object RandomLock = new();
    
    private const string NumericChars = "0123456789";
    private const string AlphabeticChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string AlphanumericChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const string HumanFriendlyChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Excludes I, O, 0, 1
    
    #region Static Methods (Original API)
    
    /// <summary>
    /// Generates a numeric reference of the specified length
    /// </summary>
    /// <param name="length">Length of the reference (1-100)</param>
    /// <param name="prefix">Optional prefix to prepend</param>
    /// <returns>Generated numeric reference</returns>
    public static string GenerateNumeric(int length, string? prefix = null)
    {
        ValidateLength(length);
        return (prefix ?? "") + GenerateRandom(length, NumericChars);
    }
    
    /// <summary>
    /// Generates an alphabetic reference using uppercase letters
    /// </summary>
    /// <param name="length">Length of the reference (1-100)</param>
    /// <param name="prefix">Optional prefix to prepend</param>
    /// <returns>Generated alphabetic reference</returns>
    public static string GenerateAlphabetic(int length, string? prefix = null)
    {
        ValidateLength(length);
        return (prefix ?? "") + GenerateRandom(length, AlphabeticChars);
    }
    
    /// <summary>
    /// Generates an alphanumeric reference combining letters and numbers
    /// </summary>
    /// <param name="length">Length of the reference (1-100)</param>
    /// <param name="prefix">Optional prefix to prepend</param>
    /// <returns>Generated alphanumeric reference</returns>
    public static string GenerateAlphanumeric(int length, string? prefix = null)
    {
        ValidateLength(length);
        return (prefix ?? "") + GenerateRandom(length, AlphanumericChars);
    }
    
    /// <summary>
    /// Generates a standard GUID
    /// </summary>
    /// <param name="prefix">Optional prefix to prepend</param>
    /// <returns>Generated GUID string</returns>
    public static string GenerateGuid(string? prefix = null)
    {
        return (prefix ?? "") + Guid.NewGuid().ToString();
    }
    
    /// <summary>
    /// Generates a cryptographically secure reference
    /// </summary>
    /// <param name="length">Length of the reference (1-100)</param>
    /// <param name="prefix">Optional prefix to prepend</param>
    /// <returns>Generated secure reference</returns>
    public static string GenerateSecure(int length, string? prefix = null)
    {
        ValidateLength(length);
        return (prefix ?? "") + GenerateSecureRandom(length, AlphanumericChars);
    }
    
    #endregion
    
    #region New Static Methods (Check Digit Support)
    
    /// <summary>
    /// Generates a numeric reference with a check digit appended
    /// </summary>
    /// <param name="length">Length of the reference (excluding check digit)</param>
    /// <param name="algorithm">Check digit algorithm to use</param>
    /// <param name="prefix">Optional prefix to prepend</param>
    /// <returns>Generated reference with check digit</returns>
    /// <example>
    /// var reference = ReferenceGenerator.GenerateNumericWithCheckDigit(9, CheckDigitAlgorithm.Luhn);
    /// // Output: "1234567893" (where 3 is the Luhn check digit)
    /// </example>
    public static string GenerateNumericWithCheckDigit(int length, CheckDigitAlgorithm algorithm = CheckDigitAlgorithm.Luhn, string? prefix = null)
    {
        ValidateLength(length);
        
        string baseNumber = GenerateRandom(length, NumericChars);
        string checkDigit = CheckDigitCalculator.Calculate(baseNumber, algorithm);
        
        return (prefix ?? "") + baseNumber + checkDigit;
    }
    
    /// <summary>
    /// Generates a human-friendly reference excluding ambiguous characters (0, O, I, 1)
    /// </summary>
    /// <param name="length">Length of the reference (1-100)</param>
    /// <param name="prefix">Optional prefix to prepend</param>
    /// <returns>Generated human-friendly reference</returns>
    public static string GenerateHumanFriendly(int length, string? prefix = null)
    {
        ValidateLength(length);
        return (prefix ?? "") + GenerateRandom(length, HumanFriendlyChars);
    }
    
    /// <summary>
    /// Generates a human-friendly reference with a check digit
    /// </summary>
    /// <param name="length">Length of the reference (excluding check digit)</param>
    /// <param name="prefix">Optional prefix to prepend</param>
    /// <returns>Generated human-friendly reference with Damm check digit</returns>
    public static string GenerateHumanFriendlyWithCheckDigit(int length, string? prefix = null)
    {
        ValidateLength(length);
        
        // Generate numeric part for check digit calculation
        string numericPart = GenerateRandom(length, NumericChars);
        char checkDigit = CheckDigitCalculator.CalculateDamm(numericPart);
        
        // Convert to human-friendly format
        string humanFriendly = GenerateRandom(length, HumanFriendlyChars);
        
        return (prefix ?? "") + humanFriendly + checkDigit;
    }
    
    #endregion
    
    #region Template-Based Generation
    
    /// <summary>
    /// Generates a reference using a template string
    /// </summary>
    /// <param name="template">Template string with placeholders</param>
    /// <param name="variables">Optional variables to substitute</param>
    /// <returns>Generated reference</returns>
    /// <example>
    /// // Simple template
    /// var reference = ReferenceGenerator.FromTemplate("{PREFIX}-{DATE:yyyyMMdd}-{NUMERIC:4}", 
    ///     new { prefix = "ORD" });
    /// // Output: "ORD-20250123-4829"
    /// 
    /// // With check digit
    /// var reference = ReferenceGenerator.FromTemplate("{NUMERIC:9}{CHECK:Luhn}");
    /// // Output: "1234567893"
    /// </example>
    public static string FromTemplate(string template, object? variables = null)
    {
        var refTemplate = new ReferenceTemplate(template);
        return refTemplate.Generate(variables);
    }
    
    /// <summary>
    /// Creates a reusable template generator
    /// </summary>
    /// <param name="template">Template string with placeholders</param>
    /// <param name="startSequence">Initial sequence value for {SEQ} placeholders</param>
    /// <returns>A ReferenceTemplate instance for generating multiple references</returns>
    /// <example>
    /// var template = ReferenceGenerator.CreateTemplate("INV-{SEQ:5}", startSequence: 1000);
    /// template.Generate(); // "INV-01000"
    /// template.Generate(); // "INV-01001"
    /// </example>
    public static ReferenceTemplate CreateTemplate(string template, long startSequence = 1)
    {
        return new ReferenceTemplate(template, startSequence);
    }
    
    #endregion
    
    #region Check Digit Validation (Convenience Methods)
    
    /// <summary>
    /// Validates a reference using the Luhn algorithm
    /// </summary>
    public static bool ValidateLuhn(string reference) => CheckDigitCalculator.ValidateLuhn(reference);
    
    /// <summary>
    /// Validates a reference using the Mod 97 algorithm (IBAN)
    /// </summary>
    public static bool ValidateMod97(string reference) => CheckDigitCalculator.ValidateMod97(reference);
    
    /// <summary>
    /// Validates a reference using the Verhoeff algorithm
    /// </summary>
    public static bool ValidateVerhoeff(string reference) => CheckDigitCalculator.ValidateVerhoeff(reference);
    
    /// <summary>
    /// Validates a reference using the Damm algorithm
    /// </summary>
    public static bool ValidateDamm(string reference) => CheckDigitCalculator.ValidateDamm(reference);
    
    /// <summary>
    /// Validates a reference using the specified algorithm
    /// </summary>
    public static bool Validate(string reference, CheckDigitAlgorithm algorithm) 
        => CheckDigitCalculator.Validate(reference, algorithm);
    
    #endregion
    
    #region Instance Methods (IReferenceGenerator)
    
    string IReferenceGenerator.GenerateNumeric(int length, string? prefix) => GenerateNumeric(length, prefix);
    string IReferenceGenerator.GenerateAlphabetic(int length, string? prefix) => GenerateAlphabetic(length, prefix);
    string IReferenceGenerator.GenerateAlphanumeric(int length, string? prefix) => GenerateAlphanumeric(length, prefix);
    string IReferenceGenerator.GenerateGuid(string? prefix) => GenerateGuid(prefix);
    string IReferenceGenerator.GenerateSecure(int length, string? prefix) => GenerateSecure(length, prefix);
    string IReferenceGenerator.GenerateNumericWithCheckDigit(int length, CheckDigitAlgorithm algorithm, string? prefix) 
        => GenerateNumericWithCheckDigit(length, algorithm, prefix);
    string IReferenceGenerator.GenerateFromTemplate(string template, object? variables) => FromTemplate(template, variables);
    
    #endregion
    
    #region Helper Methods
    
    private static string GenerateRandom(int length, string characterSet)
    {
        var sb = new StringBuilder(length);
        lock (RandomLock)
        {
            for (int i = 0; i < length; i++)
            {
                sb.Append(characterSet[SharedRandom.Next(characterSet.Length)]);
            }
        }
        return sb.ToString();
    }
    
    private static string GenerateSecureRandom(int length, string characterSet)
    {
        var result = new char[length];
        var randomBytes = new byte[length];
        
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        
        for (int i = 0; i < length; i++)
        {
            result[i] = characterSet[randomBytes[i] % characterSet.Length];
        }
        
        return new string(result);
    }
    
    private static void ValidateLength(int length)
    {
        if (length < 1 || length > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be between 1 and 100");
        }
    }
    
    #endregion
}
