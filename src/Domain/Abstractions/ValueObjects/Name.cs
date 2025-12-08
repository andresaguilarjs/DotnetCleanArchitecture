using Domain.Common;
using Domain.Constants;
using Domain.Entities.User;
using System.Text.RegularExpressions;

namespace Domain.Abstractions.ValueObjects;

/// <summary>
/// Represents a name.
/// It is recommended to use this class to store all names used in the application to avoid repetition.
/// </summary>
public abstract record Name
{
    public string Value { get; }

    protected Name(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Runs the validation for the name.
    /// It is recommended to use this method to validate the name in the derived classes.
    /// It validates if the name is empty and if it has a valid length.
    /// The valid length is between 2 and 50 characters.
    /// If the name is invalid, it returns an error indicating the problem.
    /// If the name is valid, it returns a success result.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    protected static Result ValidateName(string name, Type type)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(EmptyName(type));
        }

        if (!IsValidLength(name))
        {
            return Result.Failure(InvalidNameLength(type));
        }

        return Result.Success();
    }

    /// <summary>
    /// Checks if the name is valid.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private static bool IsValidLength(string name)
    {
        return name.Length >= 2 && name.Length <= 50;
    }

    /// <summary>
    /// Returns an error indicating that the name is empty.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static Error EmptyName(Type type)
    {
        return UserErrors.EmptyName(TransformName(type.Name));
    }

    /// <summary>
    /// Returns an error indicating that the name is invalid.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static Error InvalidNameLength(Type type)
    {
        return UserErrors.InvalidNameLength(TransformName(type.Name));
    }

    /// <summary>
    /// Transforms the name of the type to a human-readable name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private static string TransformName(string name)
    {
        return string.Join(" ", Regex.Split(name, RegularExpresions.CapitalizedWords)).ToLower();
    }
}