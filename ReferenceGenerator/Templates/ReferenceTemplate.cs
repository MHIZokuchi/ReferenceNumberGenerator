using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using ReferenceGenerator.CheckDigit;

namespace ReferenceGenerator.Templates;

/// <summary>
/// Template-based reference number generator with support for various formats
/// </summary>
/// <example>
/// // Simple usage
/// var template = new ReferenceTemplate("{PREFIX}-{DATE:yyyyMMdd}-{NUMERIC:4}");
/// string reference = template.Generate(new { prefix = "ORD" });
/// // Output: "ORD-20250123-4829"
/// 
/// // With check digit
/// var template = new ReferenceTemplate("{NUMERIC:9}{CHECK:Luhn}");
/// string reference = template.Generate();
/// // Output: "1234567893" (where 3 is the Luhn check digit)
/// 
/// // With sequence
/// var template = new ReferenceTemplate("INV-{SEQ:5}", startSequence: 1);
/// template.Generate(); // "INV-00001"
/// template.Generate(); // "INV-00002"
/// </example>
public class ReferenceTemplate
{
    private static readonly Random SharedRandom = new();
    private static readonly object RandomLock = new();
    
    private readonly List<TemplateToken> _tokens;
    private readonly string _template;
    private long _sequenceValue;
    
    /// <summary>
    /// Gets the original template string
    /// </summary>
    public string Template => _template;
    
    /// <summary>
    /// Gets the current sequence value
    /// </summary>
    public long CurrentSequence => _sequenceValue;
    
    /// <summary>
    /// Creates a new reference template
    /// </summary>
    /// <param name="template">The template string</param>
    /// <param name="startSequence">Initial value for sequence placeholders</param>
    /// <exception cref="ArgumentException">Thrown when template is invalid</exception>
    public ReferenceTemplate(string template, long startSequence = 1)
    {
        _template = template ?? throw new ArgumentNullException(nameof(template));
        _tokens = TemplateParser.Parse(template);
        _sequenceValue = startSequence;
    }
    
    /// <summary>
    /// Generates a reference using the template
    /// </summary>
    /// <param name="variables">Optional anonymous object or dictionary containing variable values</param>
    /// <param name="dateTime">Optional datetime to use for DATE placeholders (defaults to now)</param>
    /// <returns>Generated reference string</returns>
    public string Generate(object? variables = null, DateTime? dateTime = null)
    {
        var variableDict = ConvertToVariableDictionary(variables);
        var result = new StringBuilder();
        var numericParts = new StringBuilder(); // For check digit calculation
        bool hasCheckDigit = false;
        CheckDigitAlgorithm checkAlgorithm = CheckDigitAlgorithm.Luhn;
        
        // First pass: identify if we have a check digit and collect numeric parts
        foreach (var token in _tokens)
        {
            if (token.Type == TemplateTokenType.CheckDigit)
            {
                hasCheckDigit = true;
                checkAlgorithm = ParseCheckDigitAlgorithm(token.Format ?? "Luhn");
                break;
            }
        }
        
        // Second pass: generate the reference
        foreach (var token in _tokens)
        {
            string value = GenerateTokenValue(token, variableDict, dateTime ?? DateTime.Now);
            
            if (token.Type == TemplateTokenType.CheckDigit)
            {
                // Calculate check digit based on all numeric parts so far
                string numericOnly = ExtractNumericCharacters(result.ToString());
                if (string.IsNullOrEmpty(numericOnly))
                {
                    throw new InvalidOperationException("CHECK placeholder requires numeric content before it");
                }
                value = CheckDigitCalculator.Calculate(numericOnly, checkAlgorithm);
            }
            
            result.Append(value);
        }
        
        return result.ToString();
    }
    
    /// <summary>
    /// Generates multiple references
    /// </summary>
    /// <param name="count">Number of references to generate</param>
    /// <param name="variables">Optional variables for each reference</param>
    /// <returns>Array of generated references</returns>
    public string[] GenerateMany(int count, object? variables = null)
    {
        var results = new string[count];
        for (int i = 0; i < count; i++)
        {
            results[i] = Generate(variables);
        }
        return results;
    }
    
    /// <summary>
    /// Resets the sequence counter to a specific value
    /// </summary>
    public void ResetSequence(long value = 1)
    {
        Interlocked.Exchange(ref _sequenceValue, value);
    }
    
    private string GenerateTokenValue(TemplateToken token, Dictionary<string, string> variables, DateTime dateTime)
    {
        return token.Type switch
        {
            TemplateTokenType.Literal => token.Value,
            TemplateTokenType.Numeric => GenerateNumeric(int.Parse(token.Format!)),
            TemplateTokenType.Alphabetic => GenerateAlphabetic(int.Parse(token.Format!)),
            TemplateTokenType.Alphanumeric => GenerateAlphanumeric(int.Parse(token.Format!)),
            TemplateTokenType.DateTime => dateTime.ToString(token.Format),
            TemplateTokenType.Guid => GenerateGuid(token.Format),
            TemplateTokenType.Sequence => GenerateSequence(int.Parse(token.Format!)),
            TemplateTokenType.Variable => GetVariable(variables, token.Value),
            TemplateTokenType.CheckDigit => "", // Handled separately
            TemplateTokenType.Secure => GenerateSecure(int.Parse(token.Format!)),
            _ => throw new InvalidOperationException($"Unknown token type: {token.Type}")
        };
    }
    
    private static string GenerateNumeric(int length)
    {
        var sb = new StringBuilder(length);
        lock (RandomLock)
        {
            for (int i = 0; i < length; i++)
            {
                sb.Append(SharedRandom.Next(0, 10));
            }
        }
        return sb.ToString();
    }
    
    private static string GenerateAlphabetic(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var sb = new StringBuilder(length);
        lock (RandomLock)
        {
            for (int i = 0; i < length; i++)
            {
                sb.Append(chars[SharedRandom.Next(chars.Length)]);
            }
        }
        return sb.ToString();
    }
    
    private static string GenerateAlphanumeric(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var sb = new StringBuilder(length);
        lock (RandomLock)
        {
            for (int i = 0; i < length; i++)
            {
                sb.Append(chars[SharedRandom.Next(chars.Length)]);
            }
        }
        return sb.ToString();
    }
    
    private static string GenerateGuid(string? format)
    {
        var guid = Guid.NewGuid();
        return string.IsNullOrEmpty(format) ? guid.ToString() : guid.ToString(format);
    }
    
    private string GenerateSequence(int length)
    {
        long seq = Interlocked.Increment(ref _sequenceValue) - 1;
        return seq.ToString().PadLeft(length, '0');
    }
    
    private static string GetVariable(Dictionary<string, string> variables, string name)
    {
        if (variables.TryGetValue(name.ToLowerInvariant(), out string? value))
        {
            return value;
        }
        
        throw new ArgumentException($"Variable '{name}' not provided. Available variables: {string.Join(", ", variables.Keys)}");
    }
    
    private static string GenerateSecure(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var result = new char[length];
        var randomBytes = new byte[length];
        
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        
        for (int i = 0; i < length; i++)
        {
            result[i] = chars[randomBytes[i] % chars.Length];
        }
        
        return new string(result);
    }
    
    private static Dictionary<string, string> ConvertToVariableDictionary(object? variables)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        if (variables == null)
            return dict;
        
        if (variables is IDictionary<string, string> stringDict)
        {
            foreach (var kvp in stringDict)
            {
                dict[kvp.Key] = kvp.Value;
            }
            return dict;
        }
        
        if (variables is IDictionary<string, object> objDict)
        {
            foreach (var kvp in objDict)
            {
                dict[kvp.Key] = kvp.Value?.ToString() ?? "";
            }
            return dict;
        }
        
        // Use reflection for anonymous objects
        var properties = variables.GetType().GetProperties();
        foreach (var prop in properties)
        {
            var value = prop.GetValue(variables);
            dict[prop.Name] = value?.ToString() ?? "";
        }
        
        return dict;
    }
    
    private static CheckDigitAlgorithm ParseCheckDigitAlgorithm(string algorithm)
    {
        return algorithm.ToUpperInvariant() switch
        {
            "LUHN" or "MOD10" => CheckDigitAlgorithm.Luhn,
            "MOD97" or "IBAN" => CheckDigitAlgorithm.Mod97,
            "VERHOEFF" => CheckDigitAlgorithm.Verhoeff,
            "DAMM" => CheckDigitAlgorithm.Damm,
            _ => throw new ArgumentException($"Unknown check digit algorithm: {algorithm}")
        };
    }
    
    private static string ExtractNumericCharacters(string input)
    {
        var sb = new StringBuilder();
        foreach (char c in input)
        {
            if (char.IsDigit(c))
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
    
    /// <summary>
    /// Creates a template from a string (factory method)
    /// </summary>
    public static ReferenceTemplate Create(string template, long startSequence = 1)
    {
        return new ReferenceTemplate(template, startSequence);
    }
    
    /// <summary>
    /// Validates a template string without creating a template
    /// </summary>
    public static bool IsValidTemplate(string template) => TemplateParser.IsValid(template);
    
    /// <summary>
    /// Validates a template and returns any error
    /// </summary>
    public static bool TryValidateTemplate(string template, out string? errorMessage) 
        => TemplateParser.TryValidate(template, out errorMessage);
}
