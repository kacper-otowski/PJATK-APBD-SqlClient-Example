using System.Data;
using System.Data.SqlClient;
using FluentValidation;
using SqlClientExample.DTOs;
using Dapper;

namespace SqlClientExample.Endpoints;

/*
 * SqlClient with Dapper example
 * https://www.learndapper.com/
 */

public static class StudentsDapperEndpoints
{
    public static void RegisterStudentsDapperEndpoints(this WebApplication app)
    {
        var students = app.MapGroup("minimal-students-dapper");

        students.MapGet("/", GetStudents);
        students.MapGet("{id:int}", GetStudent);
        students.MapPost("/", CreateStudent);
        students.MapDelete("{id:int}", RemoveStudent);
        students.MapPut("{id:int}", ReplaceStudent);
    }

    private static IResult ReplaceStudent(IConfiguration configuration, IValidator<ReplaceStudentRequest> validator, int id, ReplaceStudentRequest request)
    {
        
        var validation = validator.Validate(request);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }
        
        using (var sqlConnection = new SqlConnection(configuration.GetConnectionString("Default")))
        {
            var affectedRows = sqlConnection.Execute(
                "UPDATE Students SET FirstName = @FirstName, LastName = @LastName, Phone = @Phone, Birthdate = @Birthdate WHERE ID = @Id",
                new
                {
                    FirstName = request.FirstName, 
                    LastName = request.LastName, 
                    Phone = request.Phone,
                    Birthdate = request.Birthdate, 
                    Id = id
                }
            );
            
            if (affectedRows == 0) return Results.NotFound();
        }

        return Results.NoContent();
    }

    private static IResult RemoveStudent(IConfiguration configuration, int id)
    {
        using (var sqlConnection = new SqlConnection(configuration.GetConnectionString("Default")))
        {
            var affectedRows = sqlConnection.Execute(
                "DELETE FROM Students WHERE ID = @Id",
                new { Id = id }
            );
            return affectedRows == 0 ? Results.NotFound() : Results.NoContent();
        }
    }

    private static IResult CreateStudent(IConfiguration configuration, IValidator<CreateStudentRequest> validator, CreateStudentRequest request)
    {
        var validation = validator.Validate(request);
        if (!validation.IsValid)
        {
            return Results.ValidationProblem(validation.ToDictionary());
        }

        using (var sqlConnection = new SqlConnection(configuration.GetConnectionString("Default")))
        {
            var id = sqlConnection.ExecuteScalar<int>(
                "INSERT INTO Students (FirstName, LastName, Phone, Birthdate) values (@FirstName, @LastName, @Phone, @Birthdate); SELECT CAST(SCOPE_IDENTITY() as int)",
                new
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Phone = request.Phone,
                    Birthdate = request.Birthdate
                }
            );

            return Results.Created($"/students-dapper/{id}", new CreateStudentResponse(id, request));
        }
    }

    private static IResult GetStudents(IConfiguration configuration)
    {
        using (var sqlConnection = new SqlConnection(configuration.GetConnectionString("Default")))
        {
            var students = sqlConnection.Query<GetStudentsResponse>("SELECT * FROM Students");
            return Results.Ok(students);
        }
    }

    private static IResult GetStudent(IConfiguration configuration, int id)
    {
        using (var sqlConnection = new SqlConnection(configuration.GetConnectionString("Default")))
        {
            var student = sqlConnection.QuerySingleOrDefault<GetStudentDetailsResponse>(
                "SELECT * FROM Students WHERE ID = @Id",
                new { Id = id }
            );

            if (student is null) return Results.NotFound();
            return Results.Ok(student);
        }
    }
}