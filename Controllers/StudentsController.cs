using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using SqlClientExample.DTOs;

namespace SqlClientExample.Controllers;

/*
 * SqlClient example with controllers
 */

[ApiController]
[Route("controller-students")]
public class StudentsController : ControllerBase
{

    private readonly IConfiguration _configuration;

    public StudentsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult GetAllStudents()
    {
        var response = new List<GetStudentsResponse>();
        using (var sqlConnection = new SqlConnection(_configuration.GetConnectionString("Default")))
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
        return Ok(response);
    }
    
    [HttpGet("{id}")]
    public IActionResult GetStudent(int id)
    {
        using var sqlConnection = new SqlConnection(_configuration.GetConnectionString("Default"));
        var sqlCommand = new SqlCommand("SELECT * FROM Students WHERE ID = @1", sqlConnection);
        sqlCommand.Parameters.AddWithValue("@1", id);
        sqlCommand.Connection.Open();

        var reader = sqlCommand.ExecuteReader();
        if (!reader.Read()) return NotFound();

        return Ok(new GetStudentDetailsResponse(
                reader.GetInt32(0), 
                reader.GetString(1),
                reader.GetString(2), 
                reader.GetString(3), 
                reader.GetDateTime(4)
            )
        );
    }
    
    [HttpPost]
    public IActionResult CreateStudent(CreateStudentRequest request)
    {
        using (var sqlConnection = new SqlConnection(_configuration.GetConnectionString("Default")))
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
            
            return Created($"students/{id}", new CreateStudentResponse((int)id, request));
        }
    }
    
    [HttpPut("{id}")]
    public IActionResult ReplaceStudent(int id, ReplaceStudentRequest request)
    {
        using (var sqlConnection = new SqlConnection(_configuration.GetConnectionString("Default")))
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
            return affectedRows == 0 ? NotFound() : NoContent();
        }
    }
    
    [HttpDelete("{id}")]
    public IActionResult RemoveStudent(int id)
    {
        using (var sqlConnection = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            var command = new SqlCommand("DELETE FROM Students WHERE ID = @1", sqlConnection);
            command.Parameters.AddWithValue("@1", id);
            command.Connection.Open();

            var affectedRows = command.ExecuteNonQuery();

            return affectedRows == 0 ? NotFound() : NoContent();
        }
    }
}