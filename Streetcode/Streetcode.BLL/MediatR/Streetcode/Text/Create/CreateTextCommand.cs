using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using FluentResults;
using Streetcode.BLL.DTO.Streetcode.TextContent.Text;

namespace Streetcode.BLL.MediatR.Streetcode.Entity.Create
{
    public record CreateTextCommand(TextCreateDTO Text) : IRequest<Result<TextDTO>>;
}
