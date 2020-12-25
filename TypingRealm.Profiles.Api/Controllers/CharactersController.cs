using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TypingRealm.Profiles.Api.Data;
using TypingRealm.Profiles.Api.Resources;

namespace TypingRealm.Profiles.Api.Controllers
{
    [Route("api/[controller]")]
    public sealed class CharactersController : OwnerResourceControllerBase<Character, CharacterResource>
    {
        private readonly IAuthorizationService _authService;
        private readonly ICharacterResourceQuery _characterResourceQuery;
        private readonly ICharacterRepository _characterRepository;

        public CharactersController(
            IAuthorizationService authService,
            ICharacterResourceQuery characterResourceQuery,
            ICharacterRepository characterRepository)
            : base(x => x.ProfileId, x => x.ProfileId)
        {
            _authService = authService;
            _characterResourceQuery = characterResourceQuery;
            _characterRepository = characterRepository;
        }

        private ProfileId ProfileId => new ProfileId(User.Identity?.Name!);

        [HttpGet]
        public ValueTask<IEnumerable<CharacterResource>> GetAllByProfileId()
        {
            return _characterResourceQuery.FindAllByProfileIdAsync(ProfileId);
        }

        [HttpGet]
        [Route("{characterId}")]
        public async ValueTask<ActionResult<CharacterResource>> GetByCharacterId(string characterId)
        {
            var characterResource = await _characterResourceQuery.FindByCharacterIdAsync(characterId);
            if (characterResource == null)
                return NotFound();

            if (!IsOwner(characterResource))
                return Forbid();

            return characterResource;
        }

        [HttpGet]
        [Route("{characterId}/info")]
        public async ValueTask<ActionResult<CharacterInfo>> GetCharacterInfo(string characterId)
        {
            var characterResource = await _characterResourceQuery.FindByCharacterIdAsync(characterId);
            if (characterResource == null)
                return NotFound();

            return new CharacterInfo(characterResource.Name);
        }

        [HttpPost]
        public async ValueTask<ActionResult> Create(CreateCharacterDto dto)
        {
            var name = new CharacterName(dto.Name);
            var characterId = await _characterRepository.NextIdAsync();

            var character = new Character(characterId, ProfileId, name);
            await _characterRepository.SaveAsync(character);

            return CreatedAtAction(
                nameof(GetByCharacterId),
                new { characterId = characterId.Value },
                character.ToCharacterResource());
        }

        [HttpPut]
        [Route("{characterId}")]
        public async ValueTask<ActionResult> Update(string characterId, UpdateCharacterDto dto)
        {
            var name = new CharacterName(dto.Name);

            var character = await _characterRepository.FindAsync(new CharacterId(characterId));
            if (character == null)
                return NotFound();

            if (!IsOwner(character))
                return Forbid();

            character.Name = name;
            await _characterRepository.SaveAsync(character);

            return NoContent();
        }

        [HttpDelete]
        [Route("{characterId}")]
        public async ValueTask<ActionResult> Delete(string characterId)
        {
            var character = await _characterRepository.FindAsync(new CharacterId(characterId));
            if (character == null)
                return NotFound();

            if (!IsOwner(character))
                return Forbid();

            character.Archive();
            await _characterRepository.SaveAsync(character);

            return NoContent();
        }

        [HttpGet]
        [Route("{characterId}/belongsToCurrentProfile")]
        public async ValueTask<ActionResult<bool>> BelongsToCurrentProfile(string characterId)
        {
            var character = await _characterRepository.FindAsync(new CharacterId(characterId));
            if (character == null)
                return NotFound();

            return IsOwner(character);
        }

        // TODO: Move to different, "world character status" API.
        [HttpGet]
        [Route("{characterId}/rope-war/{contestId}")]
        [TyrAuthorize("service")]
        public async ValueTask<ActionResult<bool>> CanJoinRopeWarContest(string characterId, string contestId)
        {
            var character = await _characterRepository.FindAsync(new CharacterId(characterId));
            if (character == null)
                return NotFound();

            // We are checking scope now, so only backend services are able to call it.
            /*if (!IsOwner(character))
                return Forbid();*/

            // We are the owner of the character.
            // Check if the character has access to given contest.
            _ = contestId;

            return true;
        }
    }

    public class TyrAuthorizeAttribute : TypeFilterAttribute
    {
        public TyrAuthorizeAttribute(string scope) : base(typeof(ScopeAuthorizationFilter))
        {
            Arguments = new object[] { scope };
        }
    }

    public class ScopeAuthorizationFilter : IAuthorizationFilter
    {
        private readonly string _scope;

        public ScopeAuthorizationFilter(string scope)
        {
            _scope = scope;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var scopeClaim = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "scope");

            if (scopeClaim == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            var hasScope = scopeClaim.Value.Split(' ').Any(scope => scope == _scope);
            if (!hasScope)
                context.Result = new ForbidResult();
        }
    }
}
