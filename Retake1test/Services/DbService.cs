using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Retake1test.Models.DTOs;
using Retake1test.Models.Exceptions;

namespace Retake1test.Services;

public class DbService : IDbService
{
    private readonly string _connectionString;
    public DbService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? string.Empty;
    }


    public async Task<GetProjectInfoByIdDto> GetProjectInfoById(int projectId)
    {
        var query = @"
        SELECT pp.ProjectId, pp.Objective, pp.StartDate, pp.EndDate, a.Name, a.OriginDate, i.InstitutionId, i.Name, i.FoundedYear, s.FirstName, s.LastName, s.HireDate, sa.Role
        FROM Preservation_Project pp
        JOIN Staff_Assignment sa ON pp.ProjectId = sa.ProjectId
        JOIN Staff s ON sa.StaffId = s.StaffId
        JOIN Artifact a ON pp.ArtifactId = a.ArtifactId
        JOIN Institution i ON a.InstitutionId = i.InstitutionId
        WHERE pp.ProjectId = @projectId;";
        
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        command.CommandText = query;
        await connection.OpenAsync();
        
        command.Parameters.AddWithValue("@projectId", projectId);
        var reader = await command.ExecuteReaderAsync();

        GetProjectInfoByIdDto? project = null;

        while (await reader.ReadAsync())
        {
            if (project is null)
            {
                project = new GetProjectInfoByIdDto()
                {
                    ProjectId = reader.GetInt32(0),
                    Objective = reader.GetString(1),
                    StartDate = reader.GetDateTime(2),
                    EndDate = await reader.IsDBNullAsync(3) ? null : reader.GetDateTime(3),
                    Artifact = new ArtifactInfoDto()
                    {
                        Name = reader.GetString(4),
                        OriginDate = reader.GetDateTime(5),
                        Institution = new InstituteInfoDto()
                        {
                            InstitutionId = reader.GetInt32(6),
                            Name = reader.GetString(7),
                            FoundedYear = reader.GetInt32(8),
                        }
                    },
                    StaffAssignments= new List<StaffAssignmentsDto>()
                };
            } 
            if(!await reader.IsDBNullAsync(9))
            {
                project.StaffAssignments.Add(new StaffAssignmentsDto()
                {
                    FirstName = reader.GetString(9),
                    LastName = reader.GetString(10),
                    HireDate = reader.GetDateTime(11),
                    Role = reader.GetString(12),
                });
            }
        }

        if (project is null)
        {
            throw new NotFoundException($"Project with id: {projectId} doesn't exist");
        }
        return project;
    }

    public async Task AddArtifactAndProject(AddArtifactCreateProjectDto dto)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        
        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            command.Parameters.Clear();
            command.CommandText = "SELECT 1 FROM Artifact WHERE ArtifactId = @artifactId;";
            command.Parameters.AddWithValue("artifactId", dto.ArtifactInfo.ArtifactId);
            var artifactIdRes = await command.ExecuteScalarAsync();

            if (artifactIdRes is not null)
            {
                throw new ConflictException($"Artifact with id: {dto.ArtifactInfo.ArtifactId} already exists");
            }

            command.Parameters.Clear();
            command.CommandText = "SELECT 1 FROM Institution WHERE InstitutionId = @institutionId;";
            command.Parameters.AddWithValue("institutionId", dto.ArtifactInfo.InstitutionId);
            var institutionIdRes = await command.ExecuteScalarAsync();

            if (institutionIdRes is null)
            {
                throw new ConflictException($"Institution with id {dto.ArtifactInfo.InstitutionId}doesn't exist");
            }

            command.Parameters.Clear();
            command.CommandText = "SELECT 1 FROM Preservation_Project WHERE ProjectId = @projectId;";
            command.Parameters.AddWithValue("projectId", dto.ProjectInfo.ProjectId);
            var projectIdRes = await command.ExecuteScalarAsync();

            if (projectIdRes is not null)
            {
                throw new ConflictException($"Project with id {dto.ProjectInfo.ProjectId} already exists");
            }

            command.Parameters.Clear();
            command.CommandText =
                @"INSERT INTO Artifact
            VALUES(@ArtifactId, @Name, @OriginDate, @InstitutionId);";
            command.Parameters.AddWithValue("@ArtifactId", dto.ArtifactInfo.ArtifactId);
            command.Parameters.AddWithValue("@Name", dto.ArtifactInfo.Name);
            command.Parameters.AddWithValue("@OriginDate", dto.ArtifactInfo.OriginDate);
            command.Parameters.AddWithValue("@InstitutionId", dto.ProjectInfo.ProjectId);

            await command.ExecuteNonQueryAsync();

            command.Parameters.Clear();
            command.CommandText =
                @"INSERT INTO Preservation_Project
            VALUES(@ProjectId, @ArtifactId, @StartDate, @EndDate, @Objective);";
            command.Parameters.AddWithValue("@ProjectId", dto.ProjectInfo.ProjectId);
            command.Parameters.AddWithValue("@ArtifactId", dto.ArtifactInfo.ArtifactId);
            command.Parameters.AddWithValue("@StartDate", dto.ProjectInfo.StartDate);
            command.Parameters.AddWithValue("@EndDate", dto.ProjectInfo.EndDate ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Objective", dto.ProjectInfo.Objective);

            await command.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}