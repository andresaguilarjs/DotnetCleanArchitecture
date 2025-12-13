using Application.Users;
using Application.Users.Commands.CreateUser;
using Application.Users.Services;
using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using FluentAssertions;
using Infrastructure.Database.Repositories.Command;
using Infrastructure.Database.Repositories.Query;

namespace Tests.Application.Users.Commands;

public class CreateUserHandlerTests : BaseTest
{
    private readonly IUserCommandRepository _userCommandRepository;
    private readonly IUserQueryRepository _userQueryRepository;
    private readonly CreateUserCommandHandler _handler;
    
    public CreateUserHandlerTests() : base()
    {
        _userQueryRepository = new UserQueryRepository(_dbContext);
        _userCommandRepository = new UserCommandRepository(_dbContext, _userQueryRepository);
        _handler = new CreateUserCommandHandler(_unitOfWorkMock.Object, _userCommandRepository, new UserService(_userQueryRepository));
    }

    /// <summary>
    /// InitializeAsync is called after the constructor to perform async setup (e.g., seeding the database).
    /// </summary>
    public override async Task InitializeAsync()
    {
        await InMemoryDatabase.UsersSeeding(_dbContext, CancellationToken.None);
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidCommand_ShouldAddUserAndCommit()
    {
        // Arrange
        UserRequest request = new ("mock@gmail.com", "Mock", "User");
        CreateUserCommand command = new (request);

        // Act
        Result<UserDTO> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be(request.Email);
        result.Value.FirstName.Should().Be(request.FirstName);
        result.Value.LastName.Should().Be(request.LastName);
        result.Value.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenCalledWithInvalidCommand_ShouldFail()
    {
        // Arrange
        UserRequest request = new("mock", "", "");
        CreateUserCommand command = new(request);

        // Act
        Result<UserDTO> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WhenCalledWithDuplicateEmail_ShouldFail()
    {
        // Arrange
        Result<IReadOnlyList<UserEntity>> users = await _userQueryRepository.ListAllAsync();
        UserEntity firstUser = users.Value.First();

        UserRequest request = new(firstUser.Email.Value, firstUser.FirstName.Value, firstUser.LastName.Value);
        CreateUserCommand command = new(request);

        // Act
        Result<UserDTO> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().HaveCount(1);
    }
}
