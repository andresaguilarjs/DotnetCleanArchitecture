namespace Application.Users;

public record UserRequest(string Email, string FirstName, string LastName, Guid? Id = null);