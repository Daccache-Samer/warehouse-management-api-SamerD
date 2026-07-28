using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Warehouse.Api.IntegrationTests.TestUtilities.Helpers;

public class TestPolicyEvaluator(IAuthorizationService authorization) : PolicyEvaluator(authorization)
{
    public override async Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
    {
        return await context.AuthenticateAsync(TestAuthHandler.AuthenticationScheme);
    }
    
    public override Task<PolicyAuthorizationResult> AuthorizeAsync(
        AuthorizationPolicy policy, 
        AuthenticateResult authenticationResult, 
        HttpContext context, 
        object? resource)
    {
        return Task.FromResult(PolicyAuthorizationResult.Success());
    }
}