using Nobisoft.Core.Lifetimes;

namespace Nobisoft.Core.Interfaces;

public interface IRandomStringGeneratorService : ITransientDependency
{
    public bool UseUpperCaseCharacters { get; set; }
    public char[] UpperCaseCharacters { get; set; }
    public bool UseLowerCaseCharacters { get; set; }
    public string GenerateUnique();
    public char[] LowerCaseCharacters { get; set; }
    public bool UseNumericCharacters { get; set; }
    public char[] NumericCharacters { get; set; }
    public bool UseSpecialCharacters { get; set; }
    public char[] SpecialCharacters { get; set; }
    public int MinUpperCaseCharacters { get; set; }
    public int MinLowerCaseCharacters { get; set; }
    public int MinNumericCharacters { get; set; }
    public int MinSpecialCharacters { get; set; }
    public string Pattern { get; set; }
    public string Generate(string pattern);
    public string Generate(int minLength, int maxLength);
    public string Generate(int fixedLength);
    public string GenerateString(int length);
    public string PatternDrivenAlgo(string pattern);
    public string SimpleGenerateAlgo(int length);
    public string GenerateAlgoWithLimits(int length);
    public void AddExistingString(string s);
    public int GetRandomInt();
    public char GetRandomCharFromArray(char[] array, ICollection<char> existentItems);
}