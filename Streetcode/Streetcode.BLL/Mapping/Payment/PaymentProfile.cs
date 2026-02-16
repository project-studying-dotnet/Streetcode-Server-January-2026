using AutoMapper;
using Streetcode.BLL.DTO.Payment;
using Streetcode.DAL.Entities.Payment;

namespace Streetcode.BLL.Mapping.Payment;

public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        CreateMap<InvoiceInfo, PaymentResponseDTO>().ReverseMap();
    }
}