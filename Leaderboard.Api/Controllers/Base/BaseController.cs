using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

namespace Leaderboard.Api.Controllers.Base;

/// <inheritdoc />
[Consumes(MediaTypeNames.Application.Json)]
[Route("api/[controller]")]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
[ApiController]
public class BaseController : ControllerBase;
