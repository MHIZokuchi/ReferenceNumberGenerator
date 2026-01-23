# ReferenceGenerator

[![NuGet](https://img.shields.io/nuget/v/ReferenceGenerator.svg)](https://www.nuget.org/packages/ReferenceGenerator/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ReferenceGenerator.svg)](https://www.nuget.org/packages/ReferenceGenerator/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A lightweight, flexible C# library for generating unique reference numbers in various formats. Perfect for creating order numbers, transaction IDs, document references, payment references, and more.

## Features

- ✅ **Multiple formats**: Numeric, alphabetic, alphanumeric, and GUID generation
- ✅ **Check digit support**: Luhn, Mod97 (IBAN), Verhoeff, and Damm algorithms
- ✅ **Template-based generation**: Powerful template syntax for custom formats
- ✅ **Human-friendly mode**: Excludes ambiguous characters (0/O, 1/I/l)
- ✅ **Cryptographically secure**: Option for security-critical applications
- ✅ **Sequence support**: Auto-incrementing references with padding
- ✅ **Cross-platform**: Supports .NET Standard 2.0, .NET 6.0, and .NET 8.0
- ✅ **DI-friendly**: Includes interface for dependency injection
- ✅ **Zero dependencies**: No external runtime dependencies

## Installation

```bash
dotnet add package ReferenceGenerator
```

Or via Package Manager Console:

```powershell
Install-Package ReferenceGenerator
```

## Quick Start

```csharp
using ReferenceGenerator;

// Generate a 10-digit numeric reference
string numericRef = ReferenceNumberGenerator.GenerateNumeric(10);
// Example: "1234567890"

// Generate with prefix
string orderRef = ReferenceNumberGenerator.GenerateNumeric(6, "ORD-");
// Example: "ORD-123456"

// Generate alphanumeric
string alphanumericRef = ReferenceNumberGenerator.GenerateAlphanumeric(12);
// Example: "A1B2C3D4E5F6"

// Generate cryptographically secure reference
string secureRef = ReferenceNumberGenerator.GenerateSecure(16);
// Example: "X7Y9Z2W4P5Q8R1S3"
```

## Check Digit Support

Generate references with built-in check digit validation - perfect for payment references, invoice numbers, and any system requiring error detection.

### Supported Algorithms

| Algorithm | Use Case | Check Digits |
|-----------|----------|--------------|
| **Luhn (Mod 10)** | Credit cards, IMEI numbers | 1 digit |
| **Mod 97** | IBAN validation (ISO 7064) | 2 digits |
| **Verhoeff** | High-security applications | 1 digit |
| **Damm** | Error detection | 1 digit |

### Usage

```csharp
using ReferenceGenerator;
using ReferenceGenerator.CheckDigit;

// Generate with Luhn check digit
string paymentRef = ReferenceNumberGenerator.GenerateNumericWithCheckDigit(9, CheckDigitAlgorithm.Luhn);
// Example: "1234567893" (where 3 is the check digit)

// Validate a reference
bool isValid = ReferenceNumberGenerator.ValidateLuhn("1234567893"); // true

// Generate with different algorithms
string verhoeffRef = ReferenceNumberGenerator.GenerateNumericWithCheckDigit(8, CheckDigitAlgorithm.Verhoeff);
string dammRef = ReferenceNumberGenerator.GenerateNumericWithCheckDigit(8, CheckDigitAlgorithm.Damm);

// Direct check digit calculation
char checkDigit = CheckDigitCalculator.CalculateLuhn("123456789"); // '7'
string mod97Digits = CheckDigitCalculator.CalculateMod97("ABCD1234"); // "XX"
```

## Template-Based Generation

Create complex reference formats using a powerful template syntax.

### Template Placeholders

| Placeholder | Description | Example |
|-------------|-------------|---------|
| `{NUMERIC:n}` | n random digits | `{NUMERIC:6}` → "482917" |
| `{ALPHA:n}` | n uppercase letters | `{ALPHA:4}` → "XKWM" |
| `{ALPHANUMERIC:n}` | n letters/digits | `{ALPHANUMERIC:8}` → "A3B7K2M9" |
| `{DATE:format}` | Formatted date/time | `{DATE:yyyyMMdd}` → "20250123" |
| `{GUID}` | Standard GUID | `{GUID}` → "550e8400-e29b-..." |
| `{GUID:N}` | GUID without dashes | `{GUID:N}` → "550e8400e29b..." |
| `{SEQ:n}` | Padded sequence | `{SEQ:5}` → "00001" |
| `{VAR:name}` | Custom variable | `{VAR:region}` → (your value) |
| `{CHECK:algo}` | Check digit | `{CHECK:Luhn}` → "3" |
| `{SECURE:n}` | Crypto-secure random | `{SECURE:10}` → "X7Y9Z2W4P5" |

### Template Examples

```csharp
using ReferenceGenerator;
using ReferenceGenerator.Templates;

// Simple template
string orderRef = ReferenceNumberGenerator.FromTemplate(
    "ORD-{DATE:yyyyMMdd}-{NUMERIC:4}");
// Example: "ORD-20250123-4829"

// With variables
string invoiceRef = ReferenceNumberGenerator.FromTemplate(
    "{VAR:prefix}-{DATE:yyMM}-{NUMERIC:6}",
    new { prefix = "INV" });
// Example: "INV-2501-482917"

// With check digit
string paymentRef = ReferenceNumberGenerator.FromTemplate(
    "PAY{NUMERIC:9}{CHECK:Luhn}");
// Example: "PAY1234567893" (Luhn-validated)

// With sequence numbers (reusable template)
var template = ReferenceNumberGenerator.CreateTemplate("TXN-{SEQ:6}", startSequence: 1000);
template.Generate(); // "TXN-001000"
template.Generate(); // "TXN-001001"
template.Generate(); // "TXN-001002"

// Complex multi-part reference
string complexRef = ReferenceNumberGenerator.FromTemplate(
    "{VAR:country}{DATE:yyMMdd}{ALPHA:2}{NUMERIC:4}{CHECK:Damm}",
    new { country = "NG" });
// Example: "NG250123XK48293"
```

### Batch Generation

```csharp
var template = new ReferenceTemplate("{ALPHANUMERIC:12}");
string[] batchRefs = template.GenerateMany(100);
// Generates 100 unique references
```

## Human-Friendly References

Generate references that avoid ambiguous characters (0/O, 1/I/l) for better readability when users need to type or read references.

```csharp
// Excludes 0, O, 1, I, l
string friendlyRef = ReferenceNumberGenerator.GenerateHumanFriendly(8);
// Example: "X7K2M9P4" (never contains ambiguous characters)
```

## Dependency Injection

The library includes an `IReferenceGenerator` interface for easy DI integration:

```csharp
// Register in DI container
services.AddSingleton<IReferenceGenerator, ReferenceNumberGenerator>();

// Inject and use
public class OrderService
{
    private readonly IReferenceGenerator _refGen;
    
    public OrderService(IReferenceGenerator refGen)
    {
        _refGen = refGen;
    }
    
    public string CreateOrder()
    {
        return _refGen.GenerateNumericWithCheckDigit(9, CheckDigitAlgorithm.Luhn, "ORD-");
    }
}
```

## API Reference

### Static Methods

```csharp
// Basic generation
ReferenceNumberGenerator.GenerateNumeric(int length, string? prefix = null)
ReferenceNumberGenerator.GenerateAlphabetic(int length, string? prefix = null)
ReferenceNumberGenerator.GenerateAlphanumeric(int length, string? prefix = null)
ReferenceNumberGenerator.GenerateGuid(string? prefix = null)
ReferenceNumberGenerator.GenerateSecure(int length, string? prefix = null)
ReferenceNumberGenerator.GenerateHumanFriendly(int length, string? prefix = null)

// With check digit
ReferenceNumberGenerator.GenerateNumericWithCheckDigit(int length, CheckDigitAlgorithm algo, string? prefix = null)

// Validation
ReferenceNumberGenerator.ValidateLuhn(string reference)
ReferenceNumberGenerator.ValidateMod97(string reference)
ReferenceNumberGenerator.ValidateVerhoeff(string reference)
ReferenceNumberGenerator.ValidateDamm(string reference)
ReferenceNumberGenerator.Validate(string reference, CheckDigitAlgorithm algorithm)

// Template-based
ReferenceNumberGenerator.FromTemplate(string template, object? variables = null)
ReferenceNumberGenerator.CreateTemplate(string template, long startSequence = 1)
```

### CheckDigitCalculator

```csharp
// Calculate check digits
CheckDigitCalculator.CalculateLuhn(string number)      // Returns char
CheckDigitCalculator.CalculateMod97(string reference)  // Returns string (2 chars)
CheckDigitCalculator.CalculateVerhoeff(string number)  // Returns char
CheckDigitCalculator.CalculateDamm(string number)      // Returns char

// Generic methods
CheckDigitCalculator.Calculate(string input, CheckDigitAlgorithm algorithm)
CheckDigitCalculator.Validate(string input, CheckDigitAlgorithm algorithm)
CheckDigitCalculator.AppendCheckDigit(string input, CheckDigitAlgorithm algorithm)
```

## Use Cases

- **E-commerce**: Order numbers, invoice references
- **Fintech**: Payment references, transaction IDs
- **Healthcare**: Patient IDs, prescription numbers
- **Logistics**: Tracking numbers, shipment references
- **Government**: Document numbers, application IDs
- **Any system** requiring unique, validated identifiers

## Contributing

Contributions are welcome! Please read our contributing guidelines before submitting pull requests.

### Development Setup

1. Clone the repository
2. Open `ReferenceGenerator.sln` in Visual Studio or your preferred IDE
3. Run tests: `dotnet test`

### Running Tests

```bash
dotnet test
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

If you encounter any issues or have questions:

1. Check existing [issues](https://github.com/MHIZokuchi/ReferenceNumberGenerator/issues)
2. Create a new issue if your problem hasn't been reported
3. Provide as much detail as possible when reporting issues

## Author

**Okwuchi Uzoigwe** - [GitHub](https://github.com/MHIZokuchi)
