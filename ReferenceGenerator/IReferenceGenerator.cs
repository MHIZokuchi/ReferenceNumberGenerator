using ReferenceGenerator.CheckDigit;

namespace ReferenceGenerator;

/// <summary>
/// Interface for reference generation, enabling dependency injection and testing
/// </summary>
public interface IReferenceGenerator
{
    /// <summary>
    /// Generates a numeric reference of the specified length
    /// </summary>
    /// <param name="length">Length of the reference</param>
    /// <param name="prefix">Optional prefix</param>
    /// <returns>Generated reference</returns>
    string GenerateNumeric(int length, string? prefix = null);
    
    /// <summary>
    /// Generates an alphabetic reference (uppercase letters only)
    /// </summary>
    /// <param name="length">Length of the reference</param>
    /// <param name="prefix">Optional prefix</param>
    /// <returns>Generated reference</returns>
    string GenerateAlphabetic(int length, string? prefix = null);
    
    /// <summary>
    /// Generates an alphanumeric reference
    /// </summary>
    /// <param name="length">Length of the reference</param>
    /// <param name="prefix">Optional prefix</param>
    /// <returns>Generated reference</returns>
    string GenerateAlphanumeric(int length, string? prefix = null);
    
    /// <summary>
    /// Generates a GUID
    /// </summary>
    /// <param name="prefix">Optional prefix</param>
    /// <returns>Generated GUID string</returns>
    string GenerateGuid(string? prefix = null);
    
    /// <summary>
    /// Generates a cryptographically secure reference
    /// </summary>
    /// <param name="length">Length of the reference</param>
    /// <param name="prefix">Optional prefix</param>
    /// <returns>Generated secure reference</returns>
    string GenerateSecure(int length, string? prefix = null);
    
    /// <summary>
    /// Generates a numeric reference with a check digit
    /// </summary>
    /// <param name="length">Length of the reference (excluding check digit)</param>
    /// <param name="algorithm">Check digit algorithm to use</param>
    /// <param name="prefix">Optional prefix</param>
    /// <returns>Generated reference with check digit</returns>
    string GenerateNumericWithCheckDigit(int length, CheckDigitAlgorithm algorithm = CheckDigitAlgorithm.Luhn, string? prefix = null);
    
    /// <summary>
    /// Generates a reference using a template
    /// </summary>
    /// <param name="template">Template string</param>
    /// <param name="variables">Optional variables for the template</param>
    /// <returns>Generated reference</returns>
    string GenerateFromTemplate(string template, object? variables = null);
}
