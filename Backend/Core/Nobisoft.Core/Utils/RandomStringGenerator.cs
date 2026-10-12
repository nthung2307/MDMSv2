using System.Collections.ObjectModel;
using System.Security.Cryptography;
using Nobisoft.Core.Interfaces;

#pragma warning disable CS0618

namespace Nobisoft.Core.Utils;

 public class RandomStringGenerator: IRandomStringGeneratorService
    {
        private string _mPattern;
        public bool RepeatCharacters;
        public bool UniqueStrings;
        private bool _mUseUpperCaseCharacters;
        private bool _mUseLowerCaseCharacters;
        private bool _mUseNumericCharacters;
        private bool _mUseSpecialCharacters;
        private int _mMinUpperCaseCharacters;
        private int _mMinLowerCaseCharacters;
        private int _mMinNumericCharacters;
        private int _mMinSpecialCharacters;
        private bool _patternDriven;
        private char[] _currentUpperCaseCharacters;
        private char[] _currentLowerCaseCharacters;
        private char[] _currentNumericCharacters;
        private char[] _currentSpecialCharacters;
        private char[] _currentGeneralCharacters;
        private readonly RNGCryptoServiceProvider _random;
        private readonly Collection<string> _existingStrings;

#pragma warning disable CS8618
        public RandomStringGenerator(
#pragma warning restore CS8618
            bool useUpperCaseCharacters = true,
            bool useLowerCaseCharacters = true,
            bool useNumericCharacters = true,
            bool useSpecialCharacters = true)
        {
            _mUseUpperCaseCharacters = useUpperCaseCharacters;
            _mUseLowerCaseCharacters = useLowerCaseCharacters;
            _mUseNumericCharacters = useNumericCharacters;
            _mUseSpecialCharacters = useSpecialCharacters;
            _currentGeneralCharacters = new char[0];
            _currentNumericCharacters = Array.Empty<char>();
            _currentLowerCaseCharacters = Array.Empty<char>();
            _currentUpperCaseCharacters = Array.Empty<char>();
            _currentSpecialCharacters = Array.Empty<char>();
            UpperCaseCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            LowerCaseCharacters = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
            NumericCharacters = "0123456789".ToCharArray();
            SpecialCharacters = ",.;:?!/@#$%^&()=+*-_{}[]<>|~".ToCharArray();
            MinUpperCaseCharacters = MinLowerCaseCharacters = MinNumericCharacters = MinSpecialCharacters = 0;
            RepeatCharacters = true;
            _patternDriven = false;
            Pattern = "";
            _random = new RNGCryptoServiceProvider();
            _existingStrings = new Collection<string>();
            
        }

        public bool UseUpperCaseCharacters
        {
            get { return _mUseUpperCaseCharacters; }
            set
            {
                _currentGeneralCharacters = _currentGeneralCharacters.Except(_currentUpperCaseCharacters).ToArray();
                if (value)
                    _currentGeneralCharacters = _currentGeneralCharacters.Concat(_currentUpperCaseCharacters).ToArray();
                _mUseUpperCaseCharacters = value;
            }
        }

        public char[] UpperCaseCharacters
        {
            get { return _currentUpperCaseCharacters; }
            set
            {
                if (UseUpperCaseCharacters)
                {
                    _currentGeneralCharacters = _currentGeneralCharacters.Except(_currentUpperCaseCharacters).ToArray();
                    _currentGeneralCharacters = _currentGeneralCharacters.Concat(value).ToArray();
                }

                _currentUpperCaseCharacters = value;
            }
        }

        public bool UseLowerCaseCharacters
        {
            get { return _mUseLowerCaseCharacters; }
            set
            {
                _currentGeneralCharacters = _currentGeneralCharacters.Except(_currentLowerCaseCharacters).ToArray();
                if (value)
                    _currentGeneralCharacters = _currentGeneralCharacters.Concat(_currentLowerCaseCharacters).ToArray();
                _mUseLowerCaseCharacters = value;
            }
        }

        public string GenerateUnique()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("=", "").Replace("+", "");
        }

        public char[] LowerCaseCharacters
        {
            get { return _currentLowerCaseCharacters; }
            set
            {
                if (UseLowerCaseCharacters)
                {
                    _currentGeneralCharacters = _currentGeneralCharacters.Except(_currentLowerCaseCharacters).ToArray();
                    _currentGeneralCharacters = _currentGeneralCharacters.Concat(value).ToArray();
                }

                _currentLowerCaseCharacters = value;
            }
        }

        public bool UseNumericCharacters
        {
            get { return _mUseNumericCharacters; }
            set
            {
                _currentGeneralCharacters = _currentGeneralCharacters.Except(_currentNumericCharacters).ToArray();
                if (value)
                    _currentGeneralCharacters = _currentGeneralCharacters.Concat(_currentNumericCharacters).ToArray();
                _mUseNumericCharacters = value;
            }
        }

        public char[] NumericCharacters
        {
            get { return _currentNumericCharacters; }
            set
            {
                if (UseNumericCharacters)
                {
                    _currentGeneralCharacters = _currentGeneralCharacters.Except(_currentNumericCharacters).ToArray();
                    _currentGeneralCharacters = _currentGeneralCharacters.Concat(value).ToArray();
                }

                _currentNumericCharacters = value;
            }
        }

        public bool UseSpecialCharacters
        {
            get { return _mUseSpecialCharacters; }
            set
            {
                _currentGeneralCharacters = _currentGeneralCharacters.Except(_currentSpecialCharacters).ToArray();
                if (value)
                    _currentGeneralCharacters = _currentGeneralCharacters.Concat(_currentSpecialCharacters).ToArray();
                _mUseSpecialCharacters = value;
            }
        }

        public char[] SpecialCharacters
        {
            get { return _currentSpecialCharacters; }
            set
            {
                if (UseSpecialCharacters)
                {
                    _currentGeneralCharacters = _currentGeneralCharacters.Except(_currentSpecialCharacters).ToArray();
                    _currentGeneralCharacters = _currentGeneralCharacters.Concat(value).ToArray();
                }

                _currentSpecialCharacters = value;
            }
        }

        public int MinUpperCaseCharacters
        {
            get { return _mMinUpperCaseCharacters; }
            set { _mMinUpperCaseCharacters = value; }
        }

        public int MinLowerCaseCharacters
        {
            get { return _mMinLowerCaseCharacters; }
            set { _mMinLowerCaseCharacters = value; }
        }

        public int MinNumericCharacters
        {
            get { return _mMinNumericCharacters; }
            set { _mMinNumericCharacters = value; }
        }

        public int MinSpecialCharacters
        {
            get { return _mMinSpecialCharacters; }
            set { _mMinSpecialCharacters = value; }
        }

        public string Pattern
        {
            get { return _mPattern; }
            set
            {
                _patternDriven = !string.Equals(value, string.Empty);
                _mPattern = value;
            }
        }

        public string Generate(string pattern)
        {
            Pattern = pattern;
            string str = GenerateString(pattern.Length);
            Pattern = "";
            return str;
        }

        public string Generate(int minLength, int maxLength)
        {
            if (maxLength < minLength)
                throw new ArgumentException("Maximal length should be grater than minimal");
            return GenerateString(minLength + GetRandomInt() % (maxLength - minLength));
        }

        public string Generate(int fixedLength)
        {
            return GenerateString(fixedLength);
        }

        public string GenerateString(int length)
        {
            if (length == 0)
                throw new ArgumentException("You can't generate a string of a zero length");
            if (!UseUpperCaseCharacters && !UseLowerCaseCharacters && (!UseNumericCharacters && !UseSpecialCharacters))
                throw new ArgumentException("There should be at least one character set in use");
            if (!RepeatCharacters && _currentGeneralCharacters.Length < length)
                throw new ArgumentException("There is not enough characters to create a string without repeats");
            var s = !_patternDriven ? (MinUpperCaseCharacters != 0 || MinLowerCaseCharacters != 0 || (MinNumericCharacters != 0 || MinSpecialCharacters != 0) ? GenerateAlgoWithLimits(length) : SimpleGenerateAlgo(length)) : PatternDrivenAlgo(Pattern);
            if (UniqueStrings && _existingStrings.Contains(s))
                return GenerateString(length);
            AddExistingString(s);
            return s;
        }

        public string PatternDrivenAlgo(string pattern)
        {
            var str = "";
            var collection = new Collection<char>();
            foreach (var ch in pattern)
            {
                char randomCharFromArray = ch switch
                {
                    '*' => GetRandomCharFromArray(_currentGeneralCharacters, collection),
                    'L' => GetRandomCharFromArray(_currentUpperCaseCharacters, collection),
                    'l' => GetRandomCharFromArray(_currentLowerCaseCharacters, collection),
                    'n' => GetRandomCharFromArray(_currentNumericCharacters, collection),
                    's' => GetRandomCharFromArray(_currentSpecialCharacters, collection),
                    _ => throw new Exception("The character '" + ch + "' is not supported")
                };

                collection.Add(randomCharFromArray);
                str += randomCharFromArray.ToString();
            }

            return str;
        }

        public string SimpleGenerateAlgo(int length)
        {
            var str = "";
            for (var index = 0; index < length; ++index)
            {
                var generalCharacter = _currentGeneralCharacters[GetRandomInt() % _currentGeneralCharacters.Length];
                if (!RepeatCharacters && str.Contains(generalCharacter))
                {
                    do
                    {
                        generalCharacter = _currentGeneralCharacters[GetRandomInt() % _currentGeneralCharacters.Length];
                    } while (str.Contains(generalCharacter));
                }

                str += generalCharacter.ToString();
            }

            return str;
        }

        public string GenerateAlgoWithLimits(int length)
        {
            if (MinUpperCaseCharacters + MinLowerCaseCharacters + MinNumericCharacters + MinSpecialCharacters > length)
                throw new ArgumentException("Sum of MinUpperCaseCharacters, MinLowerCaseCharacters, MinNumericCharacters and MinSpecialCharacters is greater than length");
            if (!RepeatCharacters && MinUpperCaseCharacters > _currentUpperCaseCharacters.Length)
                throw new ArgumentException("Can't generate a string with this number of MinUpperCaseCharacters");
            if (!RepeatCharacters && MinLowerCaseCharacters > _currentLowerCaseCharacters.Length)
                throw new ArgumentException("Can't generate a string with this number of MinLowerCaseCharacters");
            if (!RepeatCharacters && MinNumericCharacters > _currentNumericCharacters.Length)
                throw new ArgumentException("Can't generate a string with this number of MinNumericCharacters");
            if (!RepeatCharacters && MinSpecialCharacters > _currentSpecialCharacters.Length)
                throw new ArgumentException("Can't generate a string with this number of MinSpecialCharacters");
            var num = length - MinUpperCaseCharacters - MinLowerCaseCharacters - MinNumericCharacters - MinSpecialCharacters;
            var str = "";
            var collection = new Collection<char>();
            for (var index = 0; index < MinUpperCaseCharacters; ++index)
                collection.Add(GetRandomCharFromArray(UpperCaseCharacters, collection));
            for (var index = 0; index < MinLowerCaseCharacters; ++index)
                collection.Add(GetRandomCharFromArray(LowerCaseCharacters, collection));
            for (var index = 0; index < MinNumericCharacters; ++index)
                collection.Add(GetRandomCharFromArray(NumericCharacters, collection));
            for (var index = 0; index < MinSpecialCharacters; ++index)
                collection.Add(GetRandomCharFromArray(SpecialCharacters, collection));
            for (var index = 0; index < num; ++index)
                collection.Add(GetRandomCharFromArray(_currentGeneralCharacters, collection));
            for (var index1 = 0; index1 < length; ++index1)
            {
                var index2 = GetRandomInt() % collection.Count;
                var ch = collection[index2];
                collection.RemoveAt(index2);
                str += ch.ToString();
            }

            return str;
        }

        public void AddExistingString(string s)
        {
            _existingStrings.Add(s);
        }

        public int GetRandomInt()
        {
            var data = new byte[2];
            _random.GetNonZeroBytes(data);
            int num = BitConverter.ToInt16(data, 0);
            if (num < 0)
                num = -num;
            return num;
        }

        public char GetRandomCharFromArray(char[] array, ICollection<char> existentItems)
        {
            char ch;
            do
            {
                ch = array[GetRandomInt() % array.Length];
            } while (!RepeatCharacters && existentItems.Contains(ch));

            return ch;
        }

       

       
    }