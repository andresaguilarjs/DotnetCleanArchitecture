namespace Application;

public record class UserRequest(string Email, string FirstName, string LastName, Guid? Id = null);