using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Streetcode.BLL.DTO.Streetcode;

namespace Streetcode.BLL.DTO.Streetcode.Types
{
    public class StreetcodeDTOValidator : AbstractValidator<StreetcodeDTO>
    {
        public StreetcodeDTOValidator()
        {
            RuleFor(x => x.Index).InclusiveBetween(1, 9999);
            RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
            RuleFor(x => x.TransliterationUrl).NotEmpty().MaximumLength(150).Matches(@"^[a-z0-9-]+$");
            RuleFor(x => x.ShortDescription).MaximumLength(33);

            RuleFor(x => x.Teaser)
            .Custom((teaser, context) =>
            {
                if (string.IsNullOrEmpty(teaser))
                {
                    return;
                }

                int limit = teaser.Contains("\n") || teaser.Contains("\r") ? 455 : 520;
            });
        }
    }
}
