using FluentValidation.Results;

namespace Order.Application.Common.Exceptions;

public sealed class RequestValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public RequestValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation failures have occurred")
    {
        Errors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage).Distinct().ToArray());
    }
}