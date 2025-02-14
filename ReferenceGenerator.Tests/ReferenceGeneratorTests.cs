using Xunit;
using System;
using System.Text.RegularExpressions;

namespace ReferenceGenerator.Tests
{
    public class ReferenceGeneratorTests
    {
        [Theory]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(15)]
        public void GenerateNumeric_ReturnsCorrectLength(int length)
        {
            // Arrange & Act
            string result = ReferenceGenerator.GenerateNumeric(length);

            // Assert
            Assert.Equal(length, result.Length);
            Assert.Matches(new Regex(@"^\d+$"), result);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(15)]
        public void GenerateAlphabetic_ReturnsCorrectLength(int length)
        {
            // Arrange & Act
            string result = ReferenceGenerator.GenerateAlphabetic(length);

            // Assert
            Assert.Equal(length, result.Length);
            Assert.Matches(new Regex(@"^[A-Z]+$"), result);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(15)]
        public void GenerateAlphanumeric_ReturnsCorrectLength(int length)
        {
            // Arrange & Act
            string result = ReferenceGenerator.GenerateAlphanumeric(length);

            // Assert
            Assert.Equal(length, result.Length);
            Assert.Matches(new Regex(@"^[A-Z0-9]+$"), result);
        }

        [Fact]
        public void GenerateGuid_ReturnsValidGuid()
        {
            // Arrange & Act
            string result = ReferenceGenerator.GenerateGuid();

            // Assert
            Assert.True(Guid.TryParse(result, out _));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Generate_WithInvalidLength_ThrowsArgumentException(int length)
        {
            Assert.Throws<ArgumentException>(() => ReferenceGenerator.GenerateNumeric(length));
            Assert.Throws<ArgumentException>(() => ReferenceGenerator.GenerateAlphabetic(length));
            Assert.Throws<ArgumentException>(() => ReferenceGenerator.GenerateAlphanumeric(length));
            Assert.Throws<ArgumentException>(() => ReferenceGenerator.GenerateSecure(length));
        }

        [Theory]
        [InlineData("TEST-")]
        [InlineData("REF-")]
        public void Generate_WithPrefix_IncludesPrefix(string prefix)
        {
            // Arrange & Act
            string numericResult = ReferenceGenerator.GenerateNumeric(5, prefix);
            string alphabeticResult = ReferenceGenerator.GenerateAlphabetic(5, prefix);
            string alphanumericResult = ReferenceGenerator.GenerateAlphanumeric(5, prefix);

            // Assert
            Assert.StartsWith(prefix, numericResult);
            Assert.StartsWith(prefix, alphabeticResult);
            Assert.StartsWith(prefix, alphanumericResult);
        }
    }
}