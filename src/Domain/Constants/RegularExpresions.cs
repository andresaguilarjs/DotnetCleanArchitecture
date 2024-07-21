namespace Domain.Constants
{
    /// <summary>
    /// Regular expressions used throughout the application.
    /// It is recommended to use this class to store all regular expressions used in the application to avoid repetition.
    /// </summary>
    public static class RegularExpresions
    {
        /// <summary>
        /// Represents a regular expression that matches camel case characters.
        /// </summary>
        public const string CamelCase = @"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                (?<=[^A-Z])(?=[A-Z]) |
                (?<=[A-Za-z])(?=[^A-Za-z])";

        /// <summary>
        /// Represents a regular expression that matches capitalized words.
        /// </summary>
        public const string CapitalizedWords = @"(?<!^)(?=[A-Z])";
    }
}