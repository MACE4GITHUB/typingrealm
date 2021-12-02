using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Hosting;
using TypingRealm.Profiles.Api.Client;

namespace TypingRealm.Data.Api
{
    [Authorize]
    [Route("api/diagnostics")]
    public sealed class DiagnosticsController : TyrController
    {
        private readonly IServiceToServiceClient _s2sClient;

        public DiagnosticsController(IServiceToServiceClient s2sClient)
        {
            _s2sClient = s2sClient;
        }

        /// <summary>
        /// Calls Profiles API Service-protected endpoint.
        /// Accepts any anonymous calls - should be hidden from external API in
        /// future so that there's no DDOS possibility.
        /// Should always give 200.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [Route("service")]
        public async ValueTask<ActionResult<DateTime>> TryServiceToService()
        {
            return Ok(
                await _s2sClient.GetServiceToServiceCurrentDateAsync(default)
                    .ConfigureAwait(false));
        }

        /// <summary>
        /// Calls Profiles API User-protected endpoint with current token.
        /// Accepts both Service and User tokens.
        /// Should give 200 for User token, 403 for Service token (returned from
        /// downstream service).
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("profile")]
        public async ValueTask<ActionResult<DateTime>> TryProfileToService()
        {
            return Ok(
                await _s2sClient.GetProfileToServiceCurrentDateAsync(default)
                    .ConfigureAwait(false));
        }
    }
}
