namespace MyProject.Utilities.Token
{
    public class TokenGenerator
    {
        public string GenerateToken(int tokenLength = 16)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();

            var token = new char[tokenLength];
            for (var i = 0; i < tokenLength; i++)
            {
                token[i] = chars[random.Next(chars.Length)];
            }

            return new string(token);
        }
    }
}
