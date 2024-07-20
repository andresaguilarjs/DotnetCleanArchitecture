namespace Domain.Common;

/// <summary>
/// Enum that represents the error codes that can be returned by the API
/// </summary>
public enum ErrorCodes {
    Empty = 0,
    BadRequest = 400,
    ValidationError = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    Conflict = 409,
    UnprocessableEntity = 422,
    InternalServerError = 500
}