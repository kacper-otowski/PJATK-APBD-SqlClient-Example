using FluentValidation;

namespace SqlClientExample.Validators;

public static class Validators
{
    public static void RegisterValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<StudentCreateRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<StudentReplaceRequestValidator>();
    }
}