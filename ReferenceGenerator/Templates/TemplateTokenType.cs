namespace ReferenceGenerator.Templates;

/// <summary>
/// Token types supported in reference templates
/// </summary>
public enum TemplateTokenType
{
    /// <summary>
    /// Literal text that will be included as-is
    /// </summary>
    Literal,
    
    /// <summary>
    /// Numeric placeholder: {NUMERIC:length} e.g., {NUMERIC:6}
    /// </summary>
    Numeric,
    
    /// <summary>
    /// Alphabetic placeholder: {ALPHA:length} e.g., {ALPHA:4}
    /// </summary>
    Alphabetic,
    
    /// <summary>
    /// Alphanumeric placeholder: {ALPHANUMERIC:length} e.g., {ALPHANUMERIC:8}
    /// </summary>
    Alphanumeric,
    
    /// <summary>
    /// Date/time placeholder: {DATE:format} e.g., {DATE:yyyyMMdd}
    /// </summary>
    DateTime,
    
    /// <summary>
    /// GUID placeholder: {GUID} or {GUID:N} for format
    /// </summary>
    Guid,
    
    /// <summary>
    /// Sequence placeholder: {SEQ:length} e.g., {SEQ:5}
    /// </summary>
    Sequence,
    
    /// <summary>
    /// Custom variable placeholder: {VAR:name} e.g., {VAR:prefix}
    /// </summary>
    Variable,
    
    /// <summary>
    /// Check digit placeholder: {CHECK:algorithm} e.g., {CHECK:Luhn}
    /// </summary>
    CheckDigit,
    
    /// <summary>
    /// Secure random placeholder: {SECURE:length} e.g., {SECURE:10}
    /// </summary>
    Secure
}
