namespace Streetcode.BLL.DTO.Users
{
    public class RefreshTokenResponce
    {
        public string Token { get; set; }
        public DateTime ExpireAt { get; set; }
    }
}
