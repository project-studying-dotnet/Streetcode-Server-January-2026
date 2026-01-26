using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace Streetcode.BLL.MediatR.Streetcode.Text.Delete
{
    public class DeleteTextCommandValidator : AbstractValidator<DeleteTextCommand>
    {
        public DeleteTextCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("StreetcodeId must be greater than zero");
        }
    }
}
