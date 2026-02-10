namespace Streetcode.BLL.DTO.Users
{
    public class LoginResultDTO
    {
        public UserDTO User { get; set; }
        public string Token { get; set; }
        public DateTime ExpireAt { get; set; }
    }
}
