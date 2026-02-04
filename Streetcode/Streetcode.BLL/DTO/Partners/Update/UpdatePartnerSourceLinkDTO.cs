using Streetcode.DAL.Enums;

namespace Streetcode.BLL.DTO.Partners.Update
{
  public class UpdatePartnerSourceLinkDTO
  {
      public int Id { get; set; }
      public LogoTypeDTO LogoType { get; set; }
      public string TargetUrl { get; set; }
  }
}
