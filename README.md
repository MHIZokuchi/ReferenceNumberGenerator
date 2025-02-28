# ReferenceGenerator

A lightweight, flexible C# library for generating unique reference numbers in various formats. Perfect for creating order numbers, transaction IDs, document references, and more.

## Features

- Generate numeric references
- Generate alphabetic references (uppercase letters only)
- Generate alphanumeric references
- Generate GUIDs
- Generate cryptographically secure references
- Add custom prefixes to references

## Installation

Install via NuGet Package Manager:

```bash
Install-Package ReferenceGenerator
```

Or via .NET CLI:

```bash
dotnet add package ReferenceGenerator
```

## Usage

```csharp
using ReferenceGenerator;

// Generate a 10-digit numeric reference
string numericRef = ReferenceGenerator.GenerateNumeric(10);
// Example output: "1234567890"

// Generate an 8-character alphabetic reference
string alphabeticRef = ReferenceGenerator.GenerateAlphabetic(8);
// Example output: "ABCDEFGH"

// Generate a 12-character alphanumeric reference
string alphanumericRef = ReferenceGenerator.GenerateAlphanumeric(12);
// Example output: "A1B2C3D4E5F6"

// Generate a GUID
string guid = ReferenceGenerator.GenerateGuid();
// Example output: "550e8400-e29b-41d4-a716-446655440000"

// Generate references with prefixes
string orderRef = ReferenceGenerator.GenerateNumeric(6, "ORD-");
// Example output: "ORD-123456"

// Generate a cryptographically secure reference
string secureRef = ReferenceGenerator.GenerateSecure(10);
// Example output: "X7Y9Z2W4P5"
```

## Contributing

Contributions are welcome! Please read our contributing guidelines before submitting pull requests.

### Development Setup

1. Clone the repository
2. Open the solution in Visual Studio or your preferred IDE
3. Run the tests to ensure everything is working correctly

### Running Tests

```bash
dotnet test
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

If you encounter any issues or have questions, please:

1. Check the existing issues in the GitHub repository
2. Create a new issue if your problem hasn't been reported
3. Provide as much detail as possible when reporting issues

## Contributors

- [Okwuchi Uzoigwe] - Initial work
