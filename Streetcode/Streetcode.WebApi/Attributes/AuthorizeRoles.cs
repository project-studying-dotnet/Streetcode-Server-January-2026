using Microsoft.AspNetCore.Authorization;
using Streetcode.Shared.Enums;

namespace Streetcode.WebApi.Attributes
{
    public class AuthorizeRoles : AuthorizeAttribute
    {
        public AuthorizeRoles(params UserRole[] userRoles)
        {
            Roles = string.Join(",", userRoles.Select(r => r.ToString()).ToArray());
        }
    }
}
