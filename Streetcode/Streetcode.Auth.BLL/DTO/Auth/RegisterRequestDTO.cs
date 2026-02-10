namespace Streetcode.Auth.BLL.DTO.Auth
{
    public class RegisterRequestDTO
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
