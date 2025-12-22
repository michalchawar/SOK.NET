using Microsoft.AspNetCore.Authorization;
using SOK.Domain.Enums;

namespace SOK.Web.Filters
{
    /// <summary>
    /// Atrybut autoryzacji pozwalający na przekazywanie ról bezpośrednio jako enum.
    /// Użycie: [AuthorizeRoles(Role.Administrator, Role.Priest)]
    /// </summary>
    public class AuthorizeRolesAttribute : AuthorizeAttribute
    {
        public AuthorizeRolesAttribute(params Role[] roles)
        {
            if (roles == null || roles.Length == 0)
            {
                Roles = string.Join(",", Enum.GetNames(typeof(Role)));
                return;
            }

            Roles = string.Join(",", roles);
        }
    }
}
