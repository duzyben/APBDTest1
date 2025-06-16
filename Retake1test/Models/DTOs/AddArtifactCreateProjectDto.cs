namespace Retake1test.Models.DTOs;

public class AddArtifactCreateProjectDto
{
    public AddArtifactInfoDto ArtifactInfo { get; set; } = new AddArtifactInfoDto();
    public AddProjectInfoDto ProjectInfo { get; set; } = new AddProjectInfoDto();
}

public class AddArtifactInfoDto
{
    public int ArtifactId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime OriginDate { get; set; }
    public int InstitutionId { get; set;}
}

public class AddProjectInfoDto
{
    public int ProjectId { get; set; }
    public string Objective { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
}