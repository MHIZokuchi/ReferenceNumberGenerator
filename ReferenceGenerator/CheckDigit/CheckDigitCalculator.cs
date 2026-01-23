using System;

namespace ReferenceGenerator.CheckDigit;

/// <summary>
/// Provides methods for calculating and validating check digits using various algorithms
/// </summary>
public static class CheckDigitCalculator
{
    #region Luhn Algorithm (Mod 10)
    
    /// <summary>
    /// Calculates the Luhn check digit for a numeric string
    /// </summary>
    /// <param name="number">The numeric string (digits only)</param>
    /// <returns>The check digit (0-9)</returns>
    /// <exception cref="ArgumentException">Thrown when input contains non-numeric characters</exception>
    public static char CalculateLuhn(string number)
    {
        ValidateNumericInput(number, nameof(number));
        
        int sum = 0;
        bool alternate = true; // Start with true since we're calculating for the position after the last digit
        
        for (int i = number.Length - 1; i >= 0; i--)
        {
            int digit = number[i] - '0';
            
            if (alternate)
            {
                digit *= 2;
                if (digit > 9)
                    digit -= 9;
            }
            
            sum += digit;
            alternate = !alternate;
        }
        
        int checkDigit = (10 - (sum % 10)) % 10;
        return (char)('0' + checkDigit);
    }
    
    /// <summary>
    /// Validates a number with its Luhn check digit (last digit)
    /// </summary>
    /// <param name="numberWithCheckDigit">The complete number including check digit</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool ValidateLuhn(string numberWithCheckDigit)
    {
        if (string.IsNullOrEmpty(numberWithCheckDigit) || numberWithCheckDigit.Length < 2)
            return false;
            
        if (!IsNumeric(numberWithCheckDigit))
            return false;
        
        int sum = 0;
        bool alternate = false;
        
        for (int i = numberWithCheckDigit.Length - 1; i >= 0; i--)
        {
            int digit = numberWithCheckDigit[i] - '0';
            
            if (alternate)
            {
                digit *= 2;
                if (digit > 9)
                    digit -= 9;
            }
            
            sum += digit;
            alternate = !alternate;
        }
        
        return sum % 10 == 0;
    }
    
    #endregion
    
    #region Mod 97 Algorithm (IBAN)
    
    /// <summary>
    /// Calculates the Mod 97 check digits (2 digits) for IBAN-style references
    /// </summary>
    /// <param name="reference">The reference string (alphanumeric)</param>
    /// <returns>Two-character check digit string (00-97)</returns>
    public static string CalculateMod97(string reference)
    {
        if (string.IsNullOrEmpty(reference))
            throw new ArgumentException("Reference cannot be null or empty", nameof(reference));
        
        // Convert letters to numbers (A=10, B=11, ..., Z=35)
        string numericReference = ConvertToNumeric(reference + "00");
        
        // Calculate mod 97
        int remainder = Mod97(numericReference);
        int checkDigits = 98 - remainder;
        
        return checkDigits.ToString("D2");
    }
    
    /// <summary>
    /// Validates a reference with Mod 97 check digits
    /// </summary>
    /// <param name="referenceWithCheckDigits">The complete reference with check digits at the end</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool ValidateMod97(string referenceWithCheckDigits)
    {
        if (string.IsNullOrEmpty(referenceWithCheckDigits) || referenceWithCheckDigits.Length < 3)
            return false;
        
        try
        {
            string numericReference = ConvertToNumeric(referenceWithCheckDigits);
            return Mod97(numericReference) == 1;
        }
        catch
        {
            return false;
        }
    }
    
    private static string ConvertToNumeric(string input)
    {
        var result = new System.Text.StringBuilder();
        
        foreach (char c in input.ToUpperInvariant())
        {
            if (char.IsDigit(c))
            {
                result.Append(c);
            }
            else if (char.IsLetter(c))
            {
                result.Append(c - 'A' + 10);
            }
            else
            {
                throw new ArgumentException($"Invalid character '{c}' in reference");
            }
        }
        
        return result.ToString();
    }
    
    private static int Mod97(string numericString)
    {
        // Process in chunks to avoid overflow
        int remainder = 0;
        
        foreach (char c in numericString)
        {
            remainder = (remainder * 10 + (c - '0')) % 97;
        }
        
        return remainder;
    }
    
    #endregion
    
    #region Verhoeff Algorithm
    
    // Multiplication table
    private static readonly int[,] VerhoeffD = new int[,]
    {
        {0, 1, 2, 3, 4, 5, 6, 7, 8, 9},
        {1, 2, 3, 4, 0, 6, 7, 8, 9, 5},
        {2, 3, 4, 0, 1, 7, 8, 9, 5, 6},
        {3, 4, 0, 1, 2, 8, 9, 5, 6, 7},
        {4, 0, 1, 2, 3, 9, 5, 6, 7, 8},
        {5, 9, 8, 7, 6, 0, 4, 3, 2, 1},
        {6, 5, 9, 8, 7, 1, 0, 4, 3, 2},
        {7, 6, 5, 9, 8, 2, 1, 0, 4, 3},
        {8, 7, 6, 5, 9, 3, 2, 1, 0, 4},
        {9, 8, 7, 6, 5, 4, 3, 2, 1, 0}
    };
    
    // Permutation table
    private static readonly int[,] VerhoeffP = new int[,]
    {
        {0, 1, 2, 3, 4, 5, 6, 7, 8, 9},
        {1, 5, 7, 6, 2, 8, 3, 0, 9, 4},
        {5, 8, 0, 3, 7, 9, 6, 1, 4, 2},
        {8, 9, 1, 6, 0, 4, 3, 5, 2, 7},
        {9, 4, 5, 3, 1, 2, 6, 8, 7, 0},
        {4, 2, 8, 6, 5, 7, 3, 9, 0, 1},
        {2, 7, 9, 3, 8, 0, 6, 4, 1, 5},
        {7, 0, 4, 6, 9, 1, 3, 2, 5, 8}
    };
    
    // Inverse table
    private static readonly int[] VerhoeffInv = { 0, 4, 3, 2, 1, 5, 6, 7, 8, 9 };
    
    /// <summary>
    /// Calculates the Verhoeff check digit for a numeric string
    /// </summary>
    /// <param name="number">The numeric string (digits only)</param>
    /// <returns>The check digit (0-9)</returns>
    public static char CalculateVerhoeff(string number)
    {
        ValidateNumericInput(number, nameof(number));
        
        int c = 0;
        int len = number.Length;
        
        for (int i = 0; i < len; i++)
        {
            int digit = number[len - 1 - i] - '0';
            c = VerhoeffD[c, VerhoeffP[(i + 1) % 8, digit]];
        }
        
        return (char)('0' + VerhoeffInv[c]);
    }
    
    /// <summary>
    /// Validates a number with its Verhoeff check digit
    /// </summary>
    /// <param name="numberWithCheckDigit">The complete number including check digit</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool ValidateVerhoeff(string numberWithCheckDigit)
    {
        if (string.IsNullOrEmpty(numberWithCheckDigit) || !IsNumeric(numberWithCheckDigit))
            return false;
        
        int c = 0;
        int len = numberWithCheckDigit.Length;
        
        for (int i = 0; i < len; i++)
        {
            int digit = numberWithCheckDigit[len - 1 - i] - '0';
            c = VerhoeffD[c, VerhoeffP[i % 8, digit]];
        }
        
        return c == 0;
    }
    
    #endregion
    
    #region Damm Algorithm
    
    // Damm quasigroup table
    private static readonly int[,] DammTable = new int[,]
    {
        {0, 3, 1, 7, 5, 9, 8, 6, 4, 2},
        {7, 0, 9, 2, 1, 5, 4, 8, 6, 3},
        {4, 2, 0, 6, 8, 7, 1, 3, 5, 9},
        {1, 7, 5, 0, 9, 8, 3, 4, 2, 6},
        {6, 1, 2, 3, 0, 4, 5, 9, 7, 8},
        {3, 6, 7, 4, 2, 0, 9, 5, 8, 1},
        {5, 8, 6, 9, 7, 2, 0, 1, 3, 4},
        {8, 9, 4, 5, 3, 6, 2, 0, 1, 7},
        {9, 4, 3, 8, 6, 1, 7, 2, 0, 5},
        {2, 5, 8, 1, 4, 3, 6, 7, 9, 0}
    };
    
    /// <summary>
    /// Calculates the Damm check digit for a numeric string
    /// </summary>
    /// <param name="number">The numeric string (digits only)</param>
    /// <returns>The check digit (0-9)</returns>
    public static char CalculateDamm(string number)
    {
        ValidateNumericInput(number, nameof(number));
        
        int interim = 0;
        
        foreach (char c in number)
        {
            interim = DammTable[interim, c - '0'];
        }
        
        return (char)('0' + interim);
    }
    
    /// <summary>
    /// Validates a number with its Damm check digit
    /// </summary>
    /// <param name="numberWithCheckDigit">The complete number including check digit</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool ValidateDamm(string numberWithCheckDigit)
    {
        if (string.IsNullOrEmpty(numberWithCheckDigit) || !IsNumeric(numberWithCheckDigit))
            return false;
        
        int interim = 0;
        
        foreach (char c in numberWithCheckDigit)
        {
            interim = DammTable[interim, c - '0'];
        }
        
        return interim == 0;
    }
    
    #endregion
    
    #region Generic Methods
    
    /// <summary>
    /// Calculates a check digit using the specified algorithm
    /// </summary>
    /// <param name="input">The input string</param>
    /// <param name="algorithm">The algorithm to use</param>
    /// <returns>The check digit(s) as a string</returns>
    public static string Calculate(string input, CheckDigitAlgorithm algorithm)
    {
        return algorithm switch
        {
            CheckDigitAlgorithm.Luhn => CalculateLuhn(input).ToString(),
            CheckDigitAlgorithm.Mod97 => CalculateMod97(input),
            CheckDigitAlgorithm.Verhoeff => CalculateVerhoeff(input).ToString(),
            CheckDigitAlgorithm.Damm => CalculateDamm(input).ToString(),
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm))
        };
    }
    
    /// <summary>
    /// Validates an input with its check digit(s) using the specified algorithm
    /// </summary>
    /// <param name="inputWithCheckDigit">The complete input including check digit(s)</param>
    /// <param name="algorithm">The algorithm to use</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool Validate(string inputWithCheckDigit, CheckDigitAlgorithm algorithm)
    {
        return algorithm switch
        {
            CheckDigitAlgorithm.Luhn => ValidateLuhn(inputWithCheckDigit),
            CheckDigitAlgorithm.Mod97 => ValidateMod97(inputWithCheckDigit),
            CheckDigitAlgorithm.Verhoeff => ValidateVerhoeff(inputWithCheckDigit),
            CheckDigitAlgorithm.Damm => ValidateDamm(inputWithCheckDigit),
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm))
        };
    }
    
    /// <summary>
    /// Appends the appropriate check digit(s) to the input
    /// </summary>
    /// <param name="input">The input string</param>
    /// <param name="algorithm">The algorithm to use</param>
    /// <returns>The input with check digit(s) appended</returns>
    public static string AppendCheckDigit(string input, CheckDigitAlgorithm algorithm)
    {
        return input + Calculate(input, algorithm);
    }
    
    #endregion
    
    #region Helpers
    
    private static void ValidateNumericInput(string input, string paramName)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input cannot be null or empty", paramName);
        
        if (!IsNumeric(input))
            throw new ArgumentException("Input must contain only digits", paramName);
    }
    
    private static bool IsNumeric(string input)
    {
        foreach (char c in input)
        {
            if (!char.IsDigit(c))
                return false;
        }
        return true;
    }
    
    #endregion
}
