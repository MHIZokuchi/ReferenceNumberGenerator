using ReferenceGenerator.Templates;
using Xunit;
using System;
using System.Collections.Generic;

namespace ReferenceGenerator.Tests;

public class ReferenceTemplateTests
{
    #region Basic Template Parsing
    
    [Fact]
    public void Parse_SimpleNumeric_CreatesCorrectTokens()
    {
        var tokens = TemplateParser.Parse("{NUMERIC:6}");
        
        Assert.Single(tokens);
        Assert.Equal(TemplateTokenType.Numeric, tokens[0].Type);
        Assert.Equal("6", tokens[0].Format);
    }
    
    [Fact]
    public void Parse_ComplexTemplate_CreatesCorrectTokens()
    {
        var tokens = TemplateParser.Parse("ORD-{DATE:yyyyMMdd}-{NUMERIC:4}");
        
        Assert.Equal(4, tokens.Count);
        Assert.Equal(TemplateTokenType.Literal, tokens[0].Type);
        Assert.Equal("ORD-", tokens[0].Value);
        Assert.Equal(TemplateTokenType.DateTime, tokens[1].Type);
        Assert.Equal(TemplateTokenType.Literal, tokens[2].Type);
        Assert.Equal(TemplateTokenType.Numeric, tokens[3].Type);
    }
    
    [Fact]
    public void Parse_WithVariable_CreatesVariableToken()
    {
        var tokens = TemplateParser.Parse("{VAR:customerId}-{NUMERIC:4}");
        
        Assert.Equal(TemplateTokenType.Variable, tokens[0].Type);
        Assert.Equal("customerId", tokens[0].Value);
    }
    
    [Theory]
    [InlineData("{NUMERIC:6}")]
    [InlineData("{NUM:6}")]
    [InlineData("{N:6}")]
    public void Parse_NumericShortcuts_Work(string template)
    {
        var tokens = TemplateParser.Parse(template);
        Assert.Equal(TemplateTokenType.Numeric, tokens[0].Type);
    }
    
    [Theory]
    [InlineData("{ALPHA:4}")]
    [InlineData("{ALPHABETIC:4}")]
    [InlineData("{A:4}")]
    public void Parse_AlphabeticShortcuts_Work(string template)
    {
        var tokens = TemplateParser.Parse(template);
        Assert.Equal(TemplateTokenType.Alphabetic, tokens[0].Type);
    }
    
    #endregion
    
    #region Template Generation
    
    [Fact]
    public void Generate_NumericOnly_ReturnsCorrectLength()
    {
        var template = new ReferenceTemplate("{NUMERIC:10}");
        var result = template.Generate();
        
        Assert.Equal(10, result.Length);
        Assert.All(result, c => Assert.True(char.IsDigit(c)));
    }
    
    [Fact]
    public void Generate_AlphabeticOnly_ReturnsUppercaseLetters()
    {
        var template = new ReferenceTemplate("{ALPHA:8}");
        var result = template.Generate();
        
        Assert.Equal(8, result.Length);
        Assert.All(result, c => Assert.True(char.IsUpper(c) && char.IsLetter(c)));
    }
    
    [Fact]
    public void Generate_WithPrefix_IncludesPrefix()
    {
        var template = new ReferenceTemplate("ORD-{NUMERIC:6}");
        var result = template.Generate();
        
        Assert.StartsWith("ORD-", result);
        Assert.Equal(10, result.Length); // "ORD-" + 6 digits
    }
    
    [Fact]
    public void Generate_WithVariable_SubstitutesVariable()
    {
        var template = new ReferenceTemplate("{VAR:prefix}-{NUMERIC:4}");
        var result = template.Generate(new { prefix = "INV" });
        
        Assert.StartsWith("INV-", result);
    }
    
    [Fact]
    public void Generate_WithDictionary_SubstitutesVariable()
    {
        var template = new ReferenceTemplate("{VAR:prefix}-{NUMERIC:4}");
        var variables = new Dictionary<string, string> { { "prefix", "TXN" } };
        var result = template.Generate(variables);
        
        Assert.StartsWith("TXN-", result);
    }
    
    [Fact]
    public void Generate_MissingVariable_ThrowsException()
    {
        var template = new ReferenceTemplate("{VAR:prefix}-{NUMERIC:4}");
        
        Assert.Throws<ArgumentException>(() => template.Generate());
    }
    
    [Fact]
    public void Generate_WithDate_FormatsCorrectly()
    {
        var template = new ReferenceTemplate("{DATE:yyyy-MM-dd}");
        var specificDate = new DateTime(2025, 1, 23);
        var result = template.Generate(dateTime: specificDate);
        
        Assert.Equal("2025-01-23", result);
    }
    
    [Fact]
    public void Generate_WithGuid_ReturnsValidGuid()
    {
        var template = new ReferenceTemplate("{GUID}");
        var result = template.Generate();
        
        Assert.True(Guid.TryParse(result, out _));
    }
    
    [Fact]
    public void Generate_WithGuidFormat_AppliesFormat()
    {
        var template = new ReferenceTemplate("{GUID:N}");
        var result = template.Generate();
        
        Assert.Equal(32, result.Length); // GUID with N format has no dashes
        Assert.DoesNotContain("-", result);
    }
    
    #endregion
    
    #region Sequence Generation
    
    [Fact]
    public void Generate_WithSequence_IncrementsCorrectly()
    {
        var template = new ReferenceTemplate("INV-{SEQ:5}", startSequence: 1);
        
        Assert.Equal("INV-00001", template.Generate());
        Assert.Equal("INV-00002", template.Generate());
        Assert.Equal("INV-00003", template.Generate());
    }
    
    [Fact]
    public void Generate_WithSequence_PadsCorrectly()
    {
        var template = new ReferenceTemplate("{SEQ:3}", startSequence: 1);
        
        Assert.Equal("001", template.Generate());
        Assert.Equal("002", template.Generate());
    }
    
    [Fact]
    public void ResetSequence_ResetsToSpecifiedValue()
    {
        var template = new ReferenceTemplate("{SEQ:4}", startSequence: 1);
        
        template.Generate(); // 0001
        template.Generate(); // 0002
        
        template.ResetSequence(100);
        
        Assert.Equal("0100", template.Generate());
    }
    
    #endregion
    
    #region Check Digit Generation
    
    [Fact]
    public void Generate_WithLuhnCheckDigit_CreatesValidReference()
    {
        var template = new ReferenceTemplate("{NUMERIC:9}{CHECK:Luhn}");
        var result = template.Generate();
        
        Assert.Equal(10, result.Length);
        Assert.True(ReferenceNumberGenerator.ValidateLuhn(result));
    }
    
    [Fact]
    public void Generate_WithDammCheckDigit_CreatesValidReference()
    {
        var template = new ReferenceTemplate("{NUMERIC:8}{CHECK:Damm}");
        var result = template.Generate();
        
        Assert.Equal(9, result.Length);
        Assert.True(ReferenceNumberGenerator.ValidateDamm(result));
    }
    
    [Fact]
    public void Generate_WithVerhoeffCheckDigit_CreatesValidReference()
    {
        var template = new ReferenceTemplate("{NUMERIC:7}{CHECK:Verhoeff}");
        var result = template.Generate();
        
        Assert.Equal(8, result.Length);
        Assert.True(ReferenceNumberGenerator.ValidateVerhoeff(result));
    }
    
    [Fact]
    public void Generate_CheckDigitWithPrefix_CalculatesCorrectly()
    {
        // Check digit should only be calculated on numeric content
        var template = new ReferenceTemplate("REF-{NUMERIC:6}{CHECK:Luhn}");
        var result = template.Generate();
        
        // Extract numeric part and validate
        var numericPart = result.Substring(4); // After "REF-"
        Assert.True(ReferenceNumberGenerator.ValidateLuhn(numericPart));
    }
    
    #endregion
    
    #region Secure Generation
    
    [Fact]
    public void Generate_WithSecure_ReturnsCorrectLength()
    {
        var template = new ReferenceTemplate("{SECURE:16}");
        var result = template.Generate();
        
        Assert.Equal(16, result.Length);
    }
    
    #endregion
    
    #region Batch Generation
    
    [Fact]
    public void GenerateMany_CreatesMultipleReferences()
    {
        var template = new ReferenceTemplate("{NUMERIC:8}");
        var results = template.GenerateMany(10);
        
        Assert.Equal(10, results.Length);
        Assert.All(results, r => Assert.Equal(8, r.Length));
    }
    
    [Fact]
    public void GenerateMany_AllReferencesAreUnique()
    {
        var template = new ReferenceTemplate("{ALPHANUMERIC:12}");
        var results = template.GenerateMany(100);
        
        var unique = new HashSet<string>(results);
        Assert.Equal(100, unique.Count);
    }
    
    #endregion
    
    #region Validation
    
    [Fact]
    public void IsValidTemplate_ReturnsTrueForValidTemplates()
    {
        Assert.True(ReferenceTemplate.IsValidTemplate("{NUMERIC:6}"));
        Assert.True(ReferenceTemplate.IsValidTemplate("PREFIX-{DATE:yyyyMMdd}-{NUMERIC:4}"));
        Assert.True(ReferenceTemplate.IsValidTemplate("{VAR:id}{CHECK:Luhn}"));
    }
    
    [Fact]
    public void IsValidTemplate_ReturnsFalseForInvalidTemplates()
    {
        Assert.False(ReferenceTemplate.IsValidTemplate("{NUMERIC}")); // Missing length
        Assert.False(ReferenceTemplate.IsValidTemplate("{UNKNOWN:5}")); // Unknown type
        Assert.False(ReferenceTemplate.IsValidTemplate("")); // Empty
    }
    
    [Fact]
    public void TryValidateTemplate_ReturnsErrorMessage()
    {
        bool isValid = ReferenceTemplate.TryValidateTemplate("{NUMERIC}", out string? error);
        
        Assert.False(isValid);
        Assert.NotNull(error);
        Assert.Contains("length", error, StringComparison.OrdinalIgnoreCase);
    }
    
    #endregion
    
    #region Edge Cases
    
    [Fact]
    public void Generate_EmptyPrefix_WorksCorrectly()
    {
        var template = new ReferenceTemplate("{NUMERIC:4}");
        var result = template.Generate(new { prefix = "" });
        
        Assert.Equal(4, result.Length);
    }
    
    [Fact]
    public void Generate_LiteralOnly_ReturnsLiteral()
    {
        var template = new ReferenceTemplate("STATIC-VALUE");
        var result = template.Generate();
        
        Assert.Equal("STATIC-VALUE", result);
    }
    
    [Fact]
    public void Constructor_InvalidTemplate_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => new ReferenceTemplate("{INVALID}"));
    }
    
    #endregion
}
