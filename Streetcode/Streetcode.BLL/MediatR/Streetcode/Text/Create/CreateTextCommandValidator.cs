using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace Streetcode.BLL.MediatR.Streetcode.Entity.Create
{
    public class CreateTextCommandValidator : AbstractValidator<CreateTextCommand>
    {
        public CreateTextCommandValidator()
        {
            RuleFor(x => x.Text)
                .NotNull()
                .WithMessage("TextDataRequired")
                .SetValidator(new TextCreateDTOValidator());
        }
    }
}
