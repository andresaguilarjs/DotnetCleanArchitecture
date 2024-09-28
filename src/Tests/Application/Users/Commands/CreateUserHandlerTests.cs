using Application;
using Application.Users;
using Application.Users.Commands.CreateUser;
using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using Domain.Entities.User.ValueObjects;
using Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Tests.Application.Users.Commands;

public class CreateUserHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserCommandRepository> _userCommandRepositoryMock;
    private readonly Mock<IUserQueryRepository> _userQueryRepositoryMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly CrateUserCommandHandler _handler;
    public CreateUserHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userCommandRepositoryMock = new Mock<IUserCommandRepository>();
        _userQueryRepositoryMock = new Mock<IUserQueryRepository>();
        _userServiceMock = new Mock<IUserService>();
        _handler = new CrateUserCommandHandler(_unitOfWorkMock.Object, _userCommandRepositoryMock.Object, _userServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCalledWithValidCommand_ShouldAddUserAndCommit()
    {
        // Arrange
        UserRequest request = new (null, "mock@gmail.com", "Mock", "User");
        CreateUserCommand command = new (request);
        _userServiceMock.Setup(
            x => x.CreateUserEntity(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>())
            ).Returns(
                Result<UserEntity>.Success(
                    UserEntity.Create(
                        Email.Create(request.Email),
                        FirstName.Create(request.FirstName),
                        LastName.Create(request.LastName)
                    )
                )
            );

        // Act
        UserDTO result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }
}
