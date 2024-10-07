using Domain.Common;
using Domain.Entities.User;

namespace Application.Users;

public sealed class UserMapper
{
    public static UserDTO Map(UserReadEntity userEntity)
    {
        return new UserDTO(
            userEntity.Id,
            userEntity.Email,
            userEntity.FirstName,
            userEntity.LastName
        );
    }

    public static IList<UserDTO> Map(IList<UserReadEntity> userEntities)
    {
        return userEntities.Select(Map).ToList();
    }
}