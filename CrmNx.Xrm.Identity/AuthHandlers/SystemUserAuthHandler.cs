﻿using System;
using System.Threading.Tasks;
using CrmNx.Xrm.Identity.Internal;
using CrmNx.Xrm.Identity.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace CrmNx.Xrm.Identity.AuthHandlers
{
    public class SystemUserAuthHandler : AuthorizationHandler<SystemUserRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            SystemUserRequirement requirement)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.User.HasClaim(c =>
                    c.Type == CrmClaimTypes.SystemUserId &&
                    c.Issuer == CrmClaimTypes.Issuer) &&
                context.User.HasClaim(c =>
                    c.Type == CrmClaimTypes.SystemUserActive &&
                    c.Value == bool.TrueString &&
                    c.Issuer == CrmClaimTypes.Issuer))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}