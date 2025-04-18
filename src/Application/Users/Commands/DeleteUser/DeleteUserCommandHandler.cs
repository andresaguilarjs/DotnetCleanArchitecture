using Application.Abstractions.Messaging;
using Domain.Common;
using Domain.Entities.User;
using Domain.Entities.User.Interfaces;
using Domain.Interfaces;

namespace Application.Users.Commands.DeleteUser;

internal sealed class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserCommandRepository _userCommandRepository;

    public DeleteUserCommandHandler(IUnitOfWork unitOfWork, IUserCommandRepository userCommandRepository)
    {
        _unitOfWork = unitOfWork;
        _userCommandRepository = userCommandRepository;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        Result result = await _userCommandRepository.DeleteAsync(request.Id, cancellationToken);

        if (result.IsSuccess)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}
