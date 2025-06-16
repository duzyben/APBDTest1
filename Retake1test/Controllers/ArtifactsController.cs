using Microsoft.AspNetCore.Mvc;
using Retake1test.Models.DTOs;
using Retake1test.Models.Exceptions;
using Retake1test.Services;

namespace Retake1test.Controllers;

[ApiController]
[Route("[controller]")]
public class ArtifactsController : ControllerBase
{
    private readonly IDbService _dbService;
    public ArtifactsController(IDbService dbService)
    {
        _dbService = dbService;
    }
    
    [HttpPost]
    public async Task<IActionResult> AddArtifactAndProject(AddArtifactCreateProjectDto dto)
    {
        try
        {
            await _dbService.AddArtifactAndProject(dto);
        }
        catch (ConflictException e)
        {
            return Conflict(e.Message);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        return CreatedAtAction(nameof(ProjectsController.GetProjectById), new {projectId = dto.ProjectInfo.ProjectId}, dto);
    }
}