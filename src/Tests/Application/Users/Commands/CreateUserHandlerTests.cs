using Application;
using Application.Users;
using Application.Users.Commands.CreateUser;
using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using Domain.Entities.User.ValueObjects;
using Domain.Interfaces;
using FluentAssertions;
using Infrastructure.Database.Repositories.Command;
using Infrastructure.Database.Repositories.Query;
using Moq;

namespace Tests.Application.Users.Commands;

public class CreateUserHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IUserCommandRepository _userCommandRepository;
    private readonly IUserQueryRepository _userQueryRepository;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly CrateUserCommandHandler _handler;
    public CreateUserHandlerTests()
    {
        var dbContext = InMenoryDatabase.GetDbContext();

        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userQueryRepository = new UserQueryRepository(dbContext);
        _userCommandRepository = new UserCommandRepository(dbContext, _userQueryRepository);

        _userServiceMock = new Mock<IUserService>();
        _handler = new CrateUserCommandHandler(_unitOfWorkMock.Object, _userCommandRepository, _userServiceMock.Object);
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
