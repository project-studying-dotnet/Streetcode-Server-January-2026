using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Streetcode.BLL;
using Streetcode.BLL.DTO.AdditionalContent.Tag;

namespace Streetcode.BLL.DTO.Streetcode.Types
{
    public class PersonStreetcodeDTOValidator : AbstractValidator<PersonStreetcodeDTO>
    {
        public PersonStreetcodeDTOValidator()
        {
            RuleFor(x => x.FirstName).MaximumLength(50);
            RuleFor(x => x.LastName).MaximumLength(50);

            RuleForEach(x => x.Tags).SetValidator(new StreetcodeTagDTOValidator());
        }
    }
}
