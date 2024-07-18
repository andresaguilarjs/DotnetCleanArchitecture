using Domain.Common;
using Domain.Entities.User;

namespace Application.Users;

public sealed class UserMapper
{
    public static UserDTO Map(UserEntity userEntity)
    {
        return new UserDTO(
            userEntity.Id,
            userEntity.Email.Value,
            userEntity.FirstName.Value,
            userEntity.LastName.Value
        );
    }

    public static IEnumerable<UserDTO> Map(IEnumerable<UserEntity> userEntities)
    {
        return userEntities.Select(Map);
    }

    public static Error<UserDTO> Map(Error<UserEntity> error)
    {
        return new Error<UserDTO>(error.code, error.description);
    }

    public static IList<Error<UserDTO>> Map(IList<Error<UserEntity>> errors)
    {
        return errors.Select(Map).ToList();
    }
}