using Core.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        /// JWT'deki Sid claim'inden kullanıcı GUID'ini döner.
        /// </summary>
        public Guid CurrentUserGuid => User.GetUserGuid();

        /// <summary>
        /// JWT'deki Name claim'inden kullanıcı adını döner.
        /// </summary>
        public string CurrentUsername => User.GetUsername();

        /// <summary>
        /// JWT'deki rol claim'lerini döner.
        /// </summary>
        public string[] CurrentUserRoles => User.GetRoles();
    }
}