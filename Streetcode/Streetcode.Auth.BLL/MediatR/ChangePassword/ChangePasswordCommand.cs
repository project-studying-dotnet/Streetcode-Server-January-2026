using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using Streetcode.Auth.BLL.DTO.Auth;

namespace Streetcode.Auth.BLL.MediatR.ChangePassword
{
    public record ChangePasswordCommand(ChangePasswordRequestDTO Request) : IRequest<Result<Unit>>;
}