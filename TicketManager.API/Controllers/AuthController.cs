using Microsoft.AspNetCore.Mvc;

namespace TicketManager.API.Controllers;

// Container Apps' Easy Auth doesn't expose an App Service-style /.auth/me
// endpoint - identity only shows up as a header on requests that reach the
// app. This gives the frontend something to poll for "am I signed in?".
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    [HttpGet("status")]
    public IActionResult Status() =>
        Ok(new { isSignedIn = Request.Headers.ContainsKey(RequireAuthForMutationsMiddleware.ClientPrincipalIdHeader) });
}
