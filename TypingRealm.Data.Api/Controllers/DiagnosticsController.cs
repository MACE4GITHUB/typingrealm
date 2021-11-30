using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Hosting;
using TypingRealm.Profiles.Api.Client;

namespace TypingRealm.Data.Api.Controllers
{
    // TODO: Add this to all TYR controllers.
    [Authorize]
    [Route("api/diagnostics")]
    public sealed class DiagnosticsController : TyrController
    {
        private readonly IServiceToServiceClient _s2sClient;

        public DiagnosticsController(IServiceToServiceClient s2sClient)
        {
            _s2sClient = s2sClient;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("service")]
        public async ValueTask<ActionResult<DateTime>> TryServiceToService()
        {
            return Ok(await _s2sClient.GetServiceToServiceCurrentDateAsync(default));
        }

        [HttpGet]
        [Authorize]
        [Route("profile")]
        public async ValueTask<ActionResult<DateTime>> TryProfileToService()
        {
            return Ok(await _s2sClient.GetProfileToServiceCurrentDateAsync(default));
        }
    }
}
