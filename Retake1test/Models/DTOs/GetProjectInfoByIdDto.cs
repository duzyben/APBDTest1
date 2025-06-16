namespace Retake1test.Models.DTOs;

public class GetProjectInfoByIdDto
{
    public int ProjectId { get; set; }
    public String Objective { get; set; } = String.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ArtifactInfoDto? Artifact { get; set; } = new ArtifactInfoDto();
    public List<StaffAssignmentsDto> StaffAssignments { get; set; } = [];
}

public class ArtifactInfoDto
{
    public string Name { get; set; } = String.Empty;
    public DateTime OriginDate { get; set; }
    public InstituteInfoDto Institution { get; set; } = new InstituteInfoDto();
}

public class InstituteInfoDto
{
    public int InstitutionId { get; set; }
    public string Name { get; set; } = String.Empty;
    public int FoundedYear { get; set; }
}

public class StaffAssignmentsDto
{
    public string FirstName { get; set; } = String.Empty;
    public string LastName { get; set; } = String.Empty;
    public DateTime HireDate { get; set; }
    public string Role { get; set; } = String.Empty;
}