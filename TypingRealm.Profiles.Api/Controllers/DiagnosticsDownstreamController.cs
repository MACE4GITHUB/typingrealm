using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Authentication.Api;

namespace TypingRealm.Profiles.Api.Controllers
{
    [Authorize]
    [Route("api/diagnostics-downstream")]
    public sealed class DiagnosticsDownstreamController : ControllerBase
    {
        [HttpGet]
        [Authorize]
        [ServiceScoped]
        [Route("service")]
        public ActionResult<DateTime> TryServiceToService()
        {
            // Should work only with service-to-service tokens.

            return Ok(DateTime.UtcNow);
        }

        [HttpGet]
        [Authorize]
        [UserScoped]
        [Route("profile")]
        public ActionResult<DateTime> TryProfileToService()
        {
            return Ok(DateTime.UtcNow);
        }
    }
}
