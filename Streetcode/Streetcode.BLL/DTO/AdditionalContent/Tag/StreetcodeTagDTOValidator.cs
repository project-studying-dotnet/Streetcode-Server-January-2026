using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace Streetcode.BLL.DTO.AdditionalContent.Tag
{
    public class StreetcodeTagDTOValidator : AbstractValidator<StreetcodeTagDTO>
    {
        public StreetcodeTagDTOValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Index).InclusiveBetween(1, 9999);
        }
    }
}
