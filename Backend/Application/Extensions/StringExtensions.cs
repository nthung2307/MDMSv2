namespace Application.Extensions
{
    public static class StringExtensions
    {
        static string[] _vietnameseSigns = new string[]
        {
            "aAeEoOuUiIdDyY",
            "áàạảãâấầậẩẫăắằặẳẵ",
            "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
            "éèẹẻẽêếềệểễ",
            "ÉÈẸẺẼÊẾỀỆỂỄ",
            "óòọỏõôốồộổỗơớờợởỡ",
            "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
            "úùụủũưứừựửữ",
            "ÚÙỤỦŨƯỨỪỰỬỮ",
            "íìịỉĩ",
            "ÍÌỊỈĨ",
            "đ",
            "Đ",
            "ýỳỵỷỹ",
            "ÝỲỴỶỸ"
        };

        public static string RemoveVietnamese(this string input)
        {
            for (var i = 1; i < _vietnameseSigns.Length; i++)
            {
                for (var j = 0; j < _vietnameseSigns[i].Length; j++)
                    input = input.Replace(_vietnameseSigns[i][j], _vietnameseSigns[0][i - 1]);
            }

            return input;
        }
        
    }
}