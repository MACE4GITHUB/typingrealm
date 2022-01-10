using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Authentication;
using TypingRealm.Authentication.Api;
using TypingRealm.Communication;

namespace TypingRealm.Hosting
{
    public enum ServiceToServiceCallType
    {
        Unspecified = 0,

        UserProtected = 1,
        ServiceProtected = 2,
        Protected = 3,
        Anonymous = 4,

        SuperAdminProtected = 5,
        DiagnosticsAndServiceProtected = 6
    }

    public sealed record DiagnosticsCallResponse(
        string ServiceId, DateTime Date);

    public sealed record ServiceToServiceResult(
        DiagnosticsCallResponse? ServiceResponse,
        string? Errors);

    [Authorize]
    [Route("api/diagnostics")]
    public sealed class DiagnosticsController : TyrController
    {
        private static readonly string _serviceId = Guid.NewGuid().ToString();
        private readonly IServiceClient _serviceClient;

        public DiagnosticsController(IServiceClient serviceClient)
        {
            _serviceClient = serviceClient;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("service-id")]
        public string GetUniqueServiceId() => _serviceId;

        [HttpGet]
        [AllowAnonymous]
        [Route("service-to-service")]
        public ValueTask<ActionResult<ServiceToServiceResult>> TryServiceToService(string serviceName, ServiceToServiceCallType callType)
        {
            return callType switch
            {
                ServiceToServiceCallType.ServiceProtected => CallAsync(serviceName, "api/diagnostics/service-protected-call", EndpointAuthentication.Service),
                ServiceToServiceCallType.UserProtected => CallAsync(serviceName, "api/diagnostics/user-protected-call", EndpointAuthentication.Profile),
                ServiceToServiceCallType.Protected => CallAsync(serviceName, "api/diagnostics/protected-call", EndpointAuthentication.Profile),
                ServiceToServiceCallType.Anonymous => CallAsync(serviceName, "api/diagnostics/anonymous-call", EndpointAuthentication.Anonymous),
                ServiceToServiceCallType.SuperAdminProtected => CallAsync(serviceName, "api/diagnostics/superadmin-call", EndpointAuthentication.Profile),
                ServiceToServiceCallType.DiagnosticsAndServiceProtected => CallAsync(serviceName, "api/diagnostics/diagnostics-scoped-service-call", EndpointAuthentication.FromClientCredentials(new("diagnostics", "diagnostics", new[] { TyrScopes.Diagnostics, TyrScopes.Service }))),
                _ => throw new NotSupportedException("Unsupported ServiceToService call type."),
            };

            async ValueTask<ActionResult<ServiceToServiceResult>> CallAsync(string serviceName, string endpoint, EndpointAuthentication authentication)
            {
                try
                {
                    var response = await _serviceClient.GetAsync<DiagnosticsCallResponse>(
                        serviceName,
                        endpoint,
                        authentication,
                        default).ConfigureAwait(false);

                    return Ok(new ServiceToServiceResult(response, null));
                }
                catch (Exception exception)
                {
                    return BadRequest(new ServiceToServiceResult(null, exception.Message));
                }
            }
        }

        [HttpGet]
        [ServiceScoped]
        [Route("service-protected-call")]
        public DiagnosticsCallResponse GetServiceProtectedDate() => new(_serviceId, DateTime.UtcNow);

        [HttpGet]
        [UserScoped]
        [Route("user-protected-call")]
        public DiagnosticsCallResponse GetUserProtectedDate() => new(_serviceId, DateTime.UtcNow);

        [HttpGet]
        [AllowAnonymous]
        [Route("anonymous-call")]
        public DiagnosticsCallResponse GetAnonymousDate() => new(_serviceId, DateTime.UtcNow);

        [HttpGet]
        [Route("protected-call")]
        public DiagnosticsCallResponse GetProtectedDate() => new(_serviceId, DateTime.UtcNow);

        [HttpGet]
        [SuperAdminScoped]
        [Route("superadmin-call")]
        public DiagnosticsCallResponse GetSuperAdminDate() => new(_serviceId, DateTime.UtcNow);

        [HttpGet]
        [ServiceScoped, Scoped(TyrScopes.Diagnostics)]
        [Route("diagnostics-scoped-service-call")]
        public DiagnosticsCallResponse GetSuperAdminServiceDate() => new(_serviceId, DateTime.UtcNow);
    }
}
