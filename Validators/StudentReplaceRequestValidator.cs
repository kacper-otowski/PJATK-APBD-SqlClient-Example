using System.Text.RegularExpressions;
using FluentValidation;
using SqlClientExample.DTOs;

namespace SqlClientExample.Validators;

public class StudentReplaceRequestValidator : AbstractValidator<ReplaceStudentRequest>
{
    public StudentReplaceRequestValidator()
    {
        RuleFor(s => s.FirstName).MaximumLength(50).NotNull();
        RuleFor(s => s.LastName).MaximumLength(50).NotNull();
        RuleFor(s => s.Birthdate).NotNull();
        RuleFor(s => s.Phone)
            .Must(phone => Regex.IsMatch(phone, @"[0-9]+"))
            .WithMessage("The phone number must contain only digits")
            .Length(9);
    }
}