using Streetcode.Auth.BLL.DTO.Users;

namespace Streetcode.Auth.BLL.DTO.Auth
{
    public class TokenResponseDTO
    {
        public string AccessToken { get; set; }
        public DateTime AccessTokenExpiresAt { get; set; }
        public UserDTO User { get; set; }
    }
}
