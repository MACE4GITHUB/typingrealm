using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TypingRealm.Profiles.Api.Resources;

namespace TypingRealm.Profiles.Api.Controllers
{
    [Route("api/[controller]")]
    public sealed class ActivitiesController : ControllerBase
    {
        private readonly IActivityRepository _activityRepository;

        public ActivitiesController(IActivityRepository activityRepository)
        {
            _activityRepository = activityRepository;
        }

        [HttpGet("current/{characterId}")]
        public ValueTask<ActionResult<string>> GetCurrentActivity(string characterId)
        {
            var activity = _activityRepository.FindActiveActivityForCharacter(characterId);
            if (activity == null)
                return new ValueTask<ActionResult<string>>(NotFound("Activity for the character does not exist."));

            return new ValueTask<ActionResult<string>>(activity.ActivityId);
        }

        [HttpPost]
        public ValueTask<ActionResult> StartActivity(ActivityResource activity)
        {
            if (!_activityRepository.ValidateCharactersDoNotHaveActiveActivity(activity.CharacterIds))
                return new ValueTask<ActionResult>(BadRequest("Some characters are already in a different activity."));

            _activityRepository.Save(new Activity(activity.ActivityId, activity.CharacterIds));
            return new ValueTask<ActionResult>(Ok());
        }

        [HttpDelete]
        [Route("{activityId}")]
        public ValueTask<ActionResult> FinishActivity(string activityId)
        {
            var activity = _activityRepository.Find(activityId);
            if (activity == null)
                return new ValueTask<ActionResult>(NotFound("Activity is not found."));

            activity.Finish();

            _activityRepository.Save(activity);
            return new ValueTask<ActionResult>(Ok());
        }
    }
}
