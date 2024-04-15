namespace SqlClientExample.DTOs;

public record GetStudentDetailsResponse(int Id, string FirstName, string LastName, string Phone, DateTime Birthdate);