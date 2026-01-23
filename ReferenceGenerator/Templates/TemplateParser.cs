using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ReferenceGenerator.Templates;

/// <summary>
/// Parses reference templates into tokens
/// </summary>
public static class TemplateParser
{
    // Pattern to match placeholders: {TYPE} or {TYPE:parameter}
    private static readonly Regex PlaceholderPattern = new Regex(
        @"\{([A-Z]+)(?::([^}]+))?\}", 
        RegexOptions.Compiled);
    
    /// <summary>
    /// Parses a template string into a list of tokens
    /// </summary>
    /// <param name="template">The template string to parse</param>
    /// <returns>List of parsed tokens</returns>
    /// <exception cref="ArgumentException">Thrown when template is invalid</exception>
    /// <example>
    /// "{PREFIX}-{DATE:yyyyMMdd}-{NUMERIC:4}" parses to:
    /// - Variable: PREFIX
    /// - Literal: -
    /// - DateTime: yyyyMMdd
    /// - Literal: -
    /// - Numeric: 4
    /// </example>
    public static List<TemplateToken> Parse(string template)
    {
        if (string.IsNullOrEmpty(template))
            throw new ArgumentException("Template cannot be null or empty", nameof(template));
        
        var tokens = new List<TemplateToken>();
        int lastIndex = 0;
        
        foreach (Match match in PlaceholderPattern.Matches(template))
        {
            // Add any literal text before this placeholder
            if (match.Index > lastIndex)
            {
                string literal = template.Substring(lastIndex, match.Index - lastIndex);
                tokens.Add(TemplateToken.Literal(literal));
            }
            
            // Parse the placeholder
            string tokenType = match.Groups[1].Value.ToUpperInvariant();
            string? parameter = match.Groups[2].Success ? match.Groups[2].Value : null;
            
            tokens.Add(ParsePlaceholder(tokenType, parameter));
            
            lastIndex = match.Index + match.Length;
        }
        
        // Add any remaining literal text
        if (lastIndex < template.Length)
        {
            string literal = template.Substring(lastIndex);
            tokens.Add(TemplateToken.Literal(literal));
        }
        
        return tokens;
    }
    
    private static TemplateToken ParsePlaceholder(string type, string? parameter)
    {
        return type switch
        {
            "NUMERIC" or "NUM" or "N" => ParseNumericToken(parameter),
            "ALPHA" or "ALPHABETIC" or "A" => ParseAlphabeticToken(parameter),
            "ALPHANUMERIC" or "ALPHANUM" or "AN" => ParseAlphanumericToken(parameter),
            "DATE" or "DATETIME" or "DT" => ParseDateTimeToken(parameter),
            "GUID" or "UUID" => ParseGuidToken(parameter),
            "SEQ" or "SEQUENCE" => ParseSequenceToken(parameter),
            "VAR" or "VARIABLE" => ParseVariableToken(parameter),
            "CHECK" or "CHECKDIGIT" => ParseCheckDigitToken(parameter),
            "SECURE" or "SEC" => ParseSecureToken(parameter),
            "PREFIX" => TemplateToken.Variable("prefix"),
            "SUFFIX" => TemplateToken.Variable("suffix"),
            _ => throw new ArgumentException($"Unknown placeholder type: {type}")
        };
    }
    
    private static TemplateToken ParseNumericToken(string? parameter)
    {
        if (string.IsNullOrEmpty(parameter))
            throw new ArgumentException("NUMERIC placeholder requires a length parameter, e.g., {NUMERIC:6}");
        
        if (!int.TryParse(parameter, out int length) || length <= 0)
            throw new ArgumentException($"Invalid NUMERIC length: {parameter}");
        
        return TemplateToken.Numeric(length);
    }
    
    private static TemplateToken ParseAlphabeticToken(string? parameter)
    {
        if (string.IsNullOrEmpty(parameter))
            throw new ArgumentException("ALPHA placeholder requires a length parameter, e.g., {ALPHA:4}");
        
        if (!int.TryParse(parameter, out int length) || length <= 0)
            throw new ArgumentException($"Invalid ALPHA length: {parameter}");
        
        return TemplateToken.Alphabetic(length);
    }
    
    private static TemplateToken ParseAlphanumericToken(string? parameter)
    {
        if (string.IsNullOrEmpty(parameter))
            throw new ArgumentException("ALPHANUMERIC placeholder requires a length parameter, e.g., {ALPHANUMERIC:8}");
        
        if (!int.TryParse(parameter, out int length) || length <= 0)
            throw new ArgumentException($"Invalid ALPHANUMERIC length: {parameter}");
        
        return TemplateToken.Alphanumeric(length);
    }
    
    private static TemplateToken ParseDateTimeToken(string? parameter)
    {
        // Default format if not specified
        string format = string.IsNullOrEmpty(parameter) ? "yyyyMMdd" : parameter;
        return TemplateToken.DateTime(format);
    }
    
    private static TemplateToken ParseGuidToken(string? parameter)
    {
        // Parameter can be D, N, B, P, X for GUID format
        return TemplateToken.Guid(parameter);
    }
    
    private static TemplateToken ParseSequenceToken(string? parameter)
    {
        if (string.IsNullOrEmpty(parameter))
            throw new ArgumentException("SEQ placeholder requires a length parameter, e.g., {SEQ:5}");
        
        if (!int.TryParse(parameter, out int length) || length <= 0)
            throw new ArgumentException($"Invalid SEQ length: {parameter}");
        
        return TemplateToken.Sequence(length);
    }
    
    private static TemplateToken ParseVariableToken(string? parameter)
    {
        if (string.IsNullOrEmpty(parameter))
            throw new ArgumentException("VAR placeholder requires a name parameter, e.g., {VAR:customerId}");
        
        return TemplateToken.Variable(parameter);
    }
    
    private static TemplateToken ParseCheckDigitToken(string? parameter)
    {
        // Default to Luhn if not specified
        string algorithm = string.IsNullOrEmpty(parameter) ? "Luhn" : parameter;
        return TemplateToken.CheckDigit(algorithm);
    }
    
    private static TemplateToken ParseSecureToken(string? parameter)
    {
        if (string.IsNullOrEmpty(parameter))
            throw new ArgumentException("SECURE placeholder requires a length parameter, e.g., {SECURE:10}");
        
        if (!int.TryParse(parameter, out int length) || length <= 0)
            throw new ArgumentException($"Invalid SECURE length: {parameter}");
        
        return TemplateToken.Secure(length);
    }
    
    /// <summary>
    /// Validates a template string without generating
    /// </summary>
    /// <param name="template">The template to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(string template)
    {
        try
        {
            Parse(template);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Validates a template and returns any error message
    /// </summary>
    /// <param name="template">The template to validate</param>
    /// <param name="errorMessage">The error message if invalid</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool TryValidate(string template, out string? errorMessage)
    {
        try
        {
            Parse(template);
            errorMessage = null;
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }
}
