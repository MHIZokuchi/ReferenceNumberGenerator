namespace ReferenceGenerator.CheckDigit;

/// <summary>
/// Supported check digit algorithms
/// </summary>
public enum CheckDigitAlgorithm
{
    /// <summary>
    /// Luhn algorithm (Mod 10) - Used in credit cards, IMEI numbers
    /// </summary>
    Luhn,
    
    /// <summary>
    /// Mod 97 algorithm - Used in IBAN validation (ISO 7064)
    /// </summary>
    Mod97,
    
    /// <summary>
    /// Verhoeff algorithm - Detects all single-digit errors and adjacent transpositions
    /// </summary>
    Verhoeff,
    
    /// <summary>
    /// Damm algorithm - Detects all single-digit errors and adjacent transpositions
    /// </summary>
    Damm
}
