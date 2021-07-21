﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Data.Infrastructure;

namespace TypingRealm.Data.Api.Controllers
{
    [Route("api/[controller]")]
    public sealed class LocationsController : ControllerBase
    {
        private readonly ILocationRepository _locationRepository;

        public LocationsController(ILocationRepository locationRepository)
        {
            _locationRepository = locationRepository;
        }

        [HttpGet]
        [Route("{locationId}")]
        public async ValueTask<ActionResult<Location>> GetLocation(string locationId, CancellationToken cancellationToken)
        {
            var location = await _locationRepository.FindAsync(locationId, cancellationToken);
            if (location == null)
                return NotFound();

            return Ok(location);
        }
    }
}
