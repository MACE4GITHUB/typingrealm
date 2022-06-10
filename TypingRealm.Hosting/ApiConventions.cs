using Microsoft.AspNetCore.Mvc;

namespace TypingRealm.Hosting;

public static class ApiConventions
{
    [ProducesResponseType(200)]
    public static void GetAll() { }

    [ProducesResponseType(200)]
    [ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
    public static void GetCollectionByQuery() { }

    [ProducesResponseType(200)]
    [ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(404, Type = typeof(NotFoundResult))]
    public static void GetSingle() { }

    [ProducesResponseType(200)]
    [ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(404, Type = typeof(NotFoundResult))]
    [ProducesResponseType(409, Type = typeof(DomainErrorDetails))]
    public static void BusinessActionWithContent() { }

    [ProducesResponseType(203)]
    [ProducesResponseType(400, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(404, Type = typeof(NotFoundResult))]
    [ProducesResponseType(409, Type = typeof(DomainErrorDetails))]
    public static void BusinessActionNoContent() { }
}
