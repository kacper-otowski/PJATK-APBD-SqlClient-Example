using System.ComponentModel.DataAnnotations;

namespace SqlClientExample.DTOs;

public record CreateStudentRequest(
    [Required] [MaxLength(50)] string FirstName,
    [Required] [MaxLength(50)] string LastName,
    [Required] [RegularExpression("[0-9]{9}", ErrorMessage = "The phone number must contain only digits")] [Length(9, 9)] string Phone,
    [Required] DateTime Birthdate
);

public record CreateStudentResponse(int id, string FirstName, string LastName, string Phone, DateTime Birthdate)
{
    public CreateStudentResponse(int id, CreateStudentRequest request): this(id, request.FirstName, request.LastName, request.Phone, request.Birthdate){}
};