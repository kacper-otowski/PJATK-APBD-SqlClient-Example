using System.Data.SqlClient;
using FluentValidation;
using SqlClientExample.DTOs;

namespace SqlClientExample.Endpoints;

// SqlClient example with minimal api

public static class StudentsEndpoints
{
    public static void RegisterStudentsEndpoints(this WebApplication app)
    {
        var students = app.MapGroup("minimal-students");

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
            var sqlCommand = new SqlCommand(
                "UPDATE Students SET FirstName = @1, LastName = @2, Phone = @3, Birthdate = @4 WHERE ID = @5",
                sqlConnection
            );
            sqlCommand.Parameters.AddWithValue("@1", request.FirstName);
            sqlCommand.Parameters.AddWithValue("@2", request.LastName);
            sqlCommand.Parameters.AddWithValue("@3", request.Phone);
            sqlCommand.Parameters.AddWithValue("@4", request.Birthdate);
            sqlCommand.Parameters.AddWithValue("@5", id);
            sqlCommand.Connection.Open();

            var affectedRows = sqlCommand.ExecuteNonQuery();
            return affectedRows == 0 ? Results.NotFound() : Results.NoContent();
        }
    }

    private static IResult RemoveStudent(IConfiguration configuration, int id)
    {
        using (var sqlConnection = new SqlConnection(configuration.GetConnectionString("Default")))
        {
            var command = new SqlCommand("DELETE FROM Students WHERE ID = @1", sqlConnection);
            command.Parameters.AddWithValue("@1", id);
            command.Connection.Open();

            var affectedRows = command.ExecuteNonQuery();

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
            var sqlCommand = new SqlCommand(
                "INSERT INTO Students (FirstName, LastName, Phone, Birthdate) values (@1, @2, @3, @4); SELECT CAST(SCOPE_IDENTITY() as int)",
                sqlConnection
                );
            sqlCommand.Parameters.AddWithValue("@1", request.FirstName);
            sqlCommand.Parameters.AddWithValue("@2", request.LastName);
            sqlCommand.Parameters.AddWithValue("@3", request.Phone);
            sqlCommand.Parameters.AddWithValue("@4", request.Birthdate);
            sqlCommand.Connection.Open();
            
            var id = sqlCommand.ExecuteScalar();
            
            return Results.Created($"students/{id}", new CreateStudentResponse((int)id, request));
        }
    }

    private static IResult GetStudents(IConfiguration configuration)
    {
        var response = new List<GetStudentsResponse>();
        using (var sqlConnection = new SqlConnection(configuration.GetConnectionString("Default")))
        {
            var sqlCommand = new SqlCommand("SELECT * FROM Students", sqlConnection);
            sqlCommand.Connection.Open();
            var reader = sqlCommand.ExecuteReader();
            while (reader.Read())
            {
                response.Add(new GetStudentsResponse(
                        reader.GetInt32(0), 
                        reader.GetString(1), 
                        reader.GetString(2), 
                        reader.GetString(3), 
                        reader.GetDateTime(4)
                    )
                );
            }
        }
        return Results.Ok(response);
    }

    private static IResult GetStudent(IConfiguration configuration, int id)
    {
        using var sqlConnection = new SqlConnection(configuration.GetConnectionString("Default"));
        var sqlCommand = new SqlCommand("SELECT * FROM Students WHERE ID = @1", sqlConnection);
        sqlCommand.Parameters.AddWithValue("@1", id);
        sqlCommand.Connection.Open();

        var reader = sqlCommand.ExecuteReader();
        if (!reader.Read()) return Results.NotFound();

        return Results.Ok(new GetStudentDetailsResponse(
                reader.GetInt32(0), 
                reader.GetString(1),
                reader.GetString(2), 
                reader.GetString(3), 
                reader.GetDateTime(4)
            )
        );
    }
}