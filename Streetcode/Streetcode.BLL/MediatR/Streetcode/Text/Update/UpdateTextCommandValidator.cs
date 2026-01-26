using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace Streetcode.BLL.MediatR.Streetcode.Entity.Update
{
    public class UpdateTextCommandValidator : AbstractValidator<UpdateTextCommand>
    {
        public UpdateTextCommandValidator()
        {
            RuleFor(x => x.Text)
                .NotNull()
                .WithMessage("TextDataRequired")
                .SetValidator(new TextUpdateDTOValidator());
        }
    }
}
