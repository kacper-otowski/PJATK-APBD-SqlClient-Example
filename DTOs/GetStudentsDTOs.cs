namespace SqlClientExample.DTOs;

public record GetStudentsResponse(int Id, string FirstName, string LastName, string Phone, DateTime Birthdate);