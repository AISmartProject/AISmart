using AISmart.User;

namespace AISmart.Provider;

public interface IUserInformationProvider
{
    Task<bool> SaveUserExtensionInfoAsync(UserExtensionDto userExtensionDto);

    Task<UserExtensionDto> GetUserExtensionInfoByIdAsync(Guid userId);

    Task<UserExtensionDto> GetUserExtensionInfoByWalletAddressAsync(string address);
}