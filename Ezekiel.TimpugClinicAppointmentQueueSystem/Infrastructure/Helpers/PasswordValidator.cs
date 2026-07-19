public static class PasswordValidator
{
    public static List<string> Validate(string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("Password is required.");
            return errors;
        }

        if (password.Length < 12)
            errors.Add("Password must be at least 12 characters long.");

        if (!password.Any(char.IsUpper))
            errors.Add("Password must contain at least one uppercase letter.");

        if (!password.Any(char.IsLower))
            errors.Add("Password must contain at least one lowercase letter.");

        if (!password.Any(char.IsDigit))
            errors.Add("Password must contain at least one number.");

        if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            errors.Add("Password must contain at least one special character.");

        if (password.Distinct().Count() < 6)
            errors.Add("Password must contain at least 6 unique characters.");

        return errors;
    }
}