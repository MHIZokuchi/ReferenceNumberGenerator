using ReferenceGenerator.CheckDigit;
using Xunit;

namespace ReferenceGenerator.Tests;

public class CheckDigitCalculatorTests
{
    #region Luhn Algorithm Tests
    
    [Theory]
    [InlineData("7992739871", '3')] // Known test case
    [InlineData("123456789", '7')]   // Another test case
    [InlineData("0", '0')]           // Edge case
    public void CalculateLuhn_ReturnsCorrectCheckDigit(string input, char expected)
    {
        var result = CheckDigitCalculator.CalculateLuhn(input);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("79927398713")] // Valid Luhn number
    [InlineData("4532015112830366")] // Valid credit card format
    public void ValidateLuhn_ReturnsTrueForValidNumbers(string input)
    {
        Assert.True(CheckDigitCalculator.ValidateLuhn(input));
    }
    
    [Theory]
    [InlineData("79927398714")] // Invalid - wrong check digit
    [InlineData("1234567890")]   // Invalid
    public void ValidateLuhn_ReturnsFalseForInvalidNumbers(string input)
    {
        Assert.False(CheckDigitCalculator.ValidateLuhn(input));
    }
    
    [Fact]
    public void CalculateLuhn_ThrowsOnEmptyInput()
    {
        Assert.Throws<ArgumentException>(() => CheckDigitCalculator.CalculateLuhn(""));
    }
    
    [Fact]
    public void CalculateLuhn_ThrowsOnNonNumericInput()
    {
        Assert.Throws<ArgumentException>(() => CheckDigitCalculator.CalculateLuhn("ABC123"));
    }
    
    #endregion
    
    #region Mod97 Algorithm Tests
    
    [Theory]
    [InlineData("WEST12345698765432", "82")] // Example based on IBAN structure
    public void CalculateMod97_ReturnsCorrectCheckDigits(string input, string expected)
    {
        var result = CheckDigitCalculator.CalculateMod97(input);
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public void CalculateMod97_WorksWithNumericOnly()
    {
        var result = CheckDigitCalculator.CalculateMod97("123456789");
        Assert.Equal(2, result.Length);
        
        // Validate the result
        var fullNumber = "123456789" + result;
        Assert.True(CheckDigitCalculator.ValidateMod97(fullNumber));
    }
    
    [Fact]
    public void ValidateMod97_RoundTrip()
    {
        string baseRef = "TEST123456";
        string checkDigits = CheckDigitCalculator.CalculateMod97(baseRef);
        string fullRef = baseRef + checkDigits;
        
        Assert.True(CheckDigitCalculator.ValidateMod97(fullRef));
    }
    
    #endregion
    
    #region Verhoeff Algorithm Tests
    
    [Theory]
    [InlineData("236", '3')]
    [InlineData("12345", '1')]
    public void CalculateVerhoeff_ReturnsCorrectCheckDigit(string input, char expected)
    {
        var result = CheckDigitCalculator.CalculateVerhoeff(input);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("2363")]  // Valid Verhoeff number
    [InlineData("123451")] // Valid
    public void ValidateVerhoeff_ReturnsTrueForValidNumbers(string input)
    {
        Assert.True(CheckDigitCalculator.ValidateVerhoeff(input));
    }
    
    [Fact]
    public void ValidateVerhoeff_DetectsTransposition()
    {
        // Verhoeff should detect adjacent transpositions
        string valid = "2363";
        string transposed = "2633"; // Swapped middle digits
        
        Assert.True(CheckDigitCalculator.ValidateVerhoeff(valid));
        Assert.False(CheckDigitCalculator.ValidateVerhoeff(transposed));
    }
    
    #endregion
    
    #region Damm Algorithm Tests
    
    [Theory]
    [InlineData("572", '4')]
    [InlineData("12345", '0')]
    public void CalculateDamm_ReturnsCorrectCheckDigit(string input, char expected)
    {
        var result = CheckDigitCalculator.CalculateDamm(input);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("5724")]   // Valid Damm number
    [InlineData("123450")] // Valid
    public void ValidateDamm_ReturnsTrueForValidNumbers(string input)
    {
        Assert.True(CheckDigitCalculator.ValidateDamm(input));
    }
    
    [Fact]
    public void ValidateDamm_DetectsTransposition()
    {
        // Damm should detect adjacent transpositions
        string valid = "5724";
        Assert.True(CheckDigitCalculator.ValidateDamm(valid));
        
        string transposed = "5274"; // Swapped middle digits
        Assert.False(CheckDigitCalculator.ValidateDamm(transposed));
    }
    
    #endregion
    
    #region Generic Methods Tests
    
    [Fact]
    public void Calculate_WorksWithAllAlgorithms()
    {
        string input = "123456789";
        
        Assert.NotEmpty(CheckDigitCalculator.Calculate(input, CheckDigitAlgorithm.Luhn));
        Assert.NotEmpty(CheckDigitCalculator.Calculate(input, CheckDigitAlgorithm.Mod97));
        Assert.NotEmpty(CheckDigitCalculator.Calculate(input, CheckDigitAlgorithm.Verhoeff));
        Assert.NotEmpty(CheckDigitCalculator.Calculate(input, CheckDigitAlgorithm.Damm));
    }
    
    [Fact]
    public void AppendCheckDigit_RoundTrip()
    {
        string input = "123456789";
        
        foreach (var algorithm in new[] { CheckDigitAlgorithm.Luhn, CheckDigitAlgorithm.Verhoeff, CheckDigitAlgorithm.Damm })
        {
            string withCheck = CheckDigitCalculator.AppendCheckDigit(input, algorithm);
            Assert.True(CheckDigitCalculator.Validate(withCheck, algorithm), 
                $"Round-trip failed for {algorithm}");
        }
    }
    
    #endregion
}
