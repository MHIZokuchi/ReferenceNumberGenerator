namespace ReferenceGenerator.Templates;

/// <summary>
/// Represents a parsed token from a reference template
/// </summary>
public class TemplateToken
{
    /// <summary>
    /// The type of token
    /// </summary>
    public TemplateTokenType Type { get; }
    
    /// <summary>
    /// The raw value from the template (for literals) or the parameter (for placeholders)
    /// </summary>
    public string Value { get; }
    
    /// <summary>
    /// Optional format or length specification
    /// </summary>
    public string? Format { get; }
    
    /// <summary>
    /// Creates a new template token
    /// </summary>
    public TemplateToken(TemplateTokenType type, string value, string? format = null)
    {
        Type = type;
        Value = value;
        Format = format;
    }
    
    /// <summary>
    /// Creates a literal token
    /// </summary>
    public static TemplateToken Literal(string value) => new(TemplateTokenType.Literal, value);
    
    /// <summary>
    /// Creates a numeric token with specified length
    /// </summary>
    public static TemplateToken Numeric(int length) => new(TemplateTokenType.Numeric, length.ToString(), length.ToString());
    
    /// <summary>
    /// Creates an alphabetic token with specified length
    /// </summary>
    public static TemplateToken Alphabetic(int length) => new(TemplateTokenType.Alphabetic, length.ToString(), length.ToString());
    
    /// <summary>
    /// Creates an alphanumeric token with specified length
    /// </summary>
    public static TemplateToken Alphanumeric(int length) => new(TemplateTokenType.Alphanumeric, length.ToString(), length.ToString());
    
    /// <summary>
    /// Creates a date/time token with specified format
    /// </summary>
    public static TemplateToken DateTime(string format) => new(TemplateTokenType.DateTime, format, format);
    
    /// <summary>
    /// Creates a GUID token with optional format
    /// </summary>
    public static TemplateToken Guid(string? format = null) => new(TemplateTokenType.Guid, "GUID", format);
    
    /// <summary>
    /// Creates a sequence token with specified length
    /// </summary>
    public static TemplateToken Sequence(int length) => new(TemplateTokenType.Sequence, length.ToString(), length.ToString());
    
    /// <summary>
    /// Creates a variable token with specified name
    /// </summary>
    public static TemplateToken Variable(string name) => new(TemplateTokenType.Variable, name);
    
    /// <summary>
    /// Creates a check digit token with specified algorithm
    /// </summary>
    public static TemplateToken CheckDigit(string algorithm) => new(TemplateTokenType.CheckDigit, algorithm, algorithm);
    
    /// <summary>
    /// Creates a secure random token with specified length
    /// </summary>
    public static TemplateToken Secure(int length) => new(TemplateTokenType.Secure, length.ToString(), length.ToString());
}
