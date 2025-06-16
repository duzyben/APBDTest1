using Microsoft.AspNetCore.Mvc;
using Retake1test.Models.DTOs;
using Retake1test.Models.Exceptions;
using Retake1test.Services;

namespace Retake1test.Controllers;

[ApiController]
[Route("[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IDbService _dbService;
    public ProjectsController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpGet("{projectId}")]
    public async Task<IActionResult> GetProjectById(int projectId)
    {
        try
        {
            var res = await _dbService.GetProjectInfoById(projectId);
            return Ok(res);
        }
        catch(NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}