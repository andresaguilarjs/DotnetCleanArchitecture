namespace Application;

public record class UserRequest(Guid? Id, string Email, string FirstName, string LastName);