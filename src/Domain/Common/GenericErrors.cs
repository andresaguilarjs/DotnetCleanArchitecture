using System.Text.RegularExpressions;
using Domain.Abstractions;
using Domain.Constants;

namespace Domain.Common;

/// <summary>
/// Represents a collection of generic errors that can be used throughout the application.
/// It is recommended to use this class to store all generic errors used in the application to avoid repetition.
/// </summary>
public sealed class GenericErrors
{
    /// <summary>
    /// Represents an error that occurs when a resource is not found.
    /// It uses the resource ID and the object type to create the error message what makes it easier to identify the resource that was not found.
    /// </summary>
    /// <param name="resourceId"></param>
    /// <param name="objectType"></param>
    /// <returns></returns>
    public static Error NotFound(Guid resourceId, Type objectType) {
        string typeName = GetTypeName(objectType);
        return new Error(ErrorCodes.NotFound, $"The {typeName} with ID '{resourceId}' was not found.");
    }

    /// <summary>
    /// Represents an error that occurs when a parameter is null or empty.
    /// </summary>
    /// <param name="parameterName"></param>
    /// <returns></returns>
    public static Error NullOrEmpty(string parameterName) {
        return new Error(ErrorCodes.ValidationError, $"The parameter '{parameterName}' cannot be null or empty.");
    }

    /// <summary>
    /// Represent an error that occurs in unexpected moments
    /// </summary>
    /// <returns></returns>
    public static Error SomethingWhenWrong()
    {
        return new Error(ErrorCodes.InternalServerError, $"Oops! Something went wrong. Please try again in a moment.");
    }

    /// <summary>
    /// Get the type name from the object type.
    /// It will remove the generic type name from the object type to prevent show the generic type name in the error message.
    /// </summary>
    /// <param name="objectType"></param>
    /// <returns></returns>
    private static string GetTypeName(Type objectType) {
        string typeName = objectType.Name;
        if (objectType.IsGenericType) {
            typeName = objectType.GetGenericArguments().First().Name;
        }

        Regex regularExpresion = new (RegularExpresions.CamelCase, RegexOptions.IgnorePatternWhitespace);

        Match? match = regularExpresion.Matches(typeName).LastOrDefault();
        if (match is not null) {
            typeName = typeName.Substring(0, match.Index);
        }

        return typeName;
    }
}