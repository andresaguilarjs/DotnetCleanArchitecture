using Application;
using Application.Users;
using Application.Users.Commands.CreateUser;
using Application.Users.Services;
using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using FluentAssertions;
using Infrastructure.Database.Repositories.Command;
using Infrastructure.Database.Repositories.Query;
using Moq;

namespace Tests.Application.Users.Commands;

public class CreateUserHandlerTests : BaseTest
{
    private readonly IUserCommandRepository _userCommandRepository;
    private readonly IUserQueryRepository _userQueryRepository;
    private readonly CreateUserCommandHandler _handler;
    public CreateUserHandlerTests() : base()
    {
        _userCommandRepository = new UserCommandRepository(_writeDbContext);
        _userQueryRepository = new UserQueryRepository(_readDbContext);
        _handler = new CreateUserCommandHandler(_unitOfWorkMock.Object, _userCommandRepository, new UserService(_userQueryRepository));

        _inMemoryDatabase.UserSeeding(_writeDbContext).Wait();
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidCommand_ShouldAddUserAndCommit()
    {
        // Arrange
        UserRequest request = new ("mock2@gmail.com", "Mock", "User");
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
    public async Task Handle_WhenCalledWithInValidCommand_ShouldFail()
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
}
