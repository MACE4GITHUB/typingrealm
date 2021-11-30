﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TypingRealm.Profiles;

namespace TypingRealm.Authentication
{
    public sealed class ProfileTypeAuthorizationFilter : IAuthorizationFilter
    {
        private readonly ProfileType _profileType;

        public ProfileTypeAuthorizationFilter(ProfileType profileType)
        {
            _profileType = profileType;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var profile = ProfileHelpers.GetProfileForUser(context.HttpContext.User);

            if (profile.Type != _profileType)
                context.Result = new ForbidResult();
        }
    }
}
