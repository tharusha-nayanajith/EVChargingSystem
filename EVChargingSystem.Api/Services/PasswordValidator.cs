namespace EVChargingSystem.Api.Services
{
    public static class PasswordValidator
    {
        public static (bool IsValid, string Message) Validate(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password cannot be empty.");

            var errors = new List<string>();

            if (password.Length < 8)
                errors.Add("at least 8 characters");
            if (!password.Any(char.IsUpper))
                errors.Add("one uppercase letter");
            if (!password.Any(char.IsLower))
                errors.Add("one lowercase letter");
            if (!password.Any(char.IsDigit))
                errors.Add("one digit");
            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
                errors.Add("one special character");

            if (errors.Count == 0)
                return (true, string.Empty);

            return (false, "Password must contain " + string.Join(", ", errors) + ".");
        }
    }
}

