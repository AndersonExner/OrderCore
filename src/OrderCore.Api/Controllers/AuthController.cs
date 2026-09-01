using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderCore.Application.Auth.Commands;
using OrderCore.Application.Auth.Dtos;

namespace OrderCore.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly RegisterUserService _registerUserService;
    private readonly LoginService _loginService;

    public AuthController(
        RegisterUserService registerUserService,
        LoginService loginService)
    {
        _registerUserService = registerUserService;
        _loginService = loginService;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _registerUserService.ExecuteAsync(request, cancellationToken);

        return Created(string.Empty, response);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _loginService.ExecuteAsync(request, cancellationToken);

        if (response is null)
            return Unauthorized();

        return Ok(response);
    }
}
