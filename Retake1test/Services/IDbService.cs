using Retake1test.Models.DTOs;

namespace Retake1test.Services;

public interface IDbService
{
    Task<GetProjectInfoByIdDto> GetProjectInfoById(int projectId);

    Task AddArtifactAndProject(AddArtifactCreateProjectDto dto);
}