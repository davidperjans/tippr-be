using FluentValidation;
using MediatR;
using System.Reflection;

namespace Application.Common.Behaviors
{
    public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);

            var results = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = results
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .ToList();

            if (failures.Count == 0)
            {
                return await next();
            }

            var errors = failures
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ErrorMessage).Distinct().ToArray()
                );

            return CreateValidationResult(errors);
        }

        private static TResponse CreateValidationResult(Dictionary<string, string[]> errors)
        {
            const string message = "One or more validation errors occurred.";
            const string code = "validation.failed";

            // Result (non-generic)
            if (typeof(TResponse) == typeof(Result))
            {
                return (TResponse)(object)Result.Validation(errors, message, code);
            }

            // Result<TData>
            if (typeof(TResponse).IsGenericType &&
                typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var dataType = typeof(TResponse).GetGenericArguments()[0];

                // Find: public static Result<TData> Validation(Dictionary<string,string[]>, string message, string? code)
                var resultGenericType = typeof(Result<>).MakeGenericType(dataType);

                var method = resultGenericType.GetMethod(
                    name: "Validation",
                    bindingAttr: BindingFlags.Public | BindingFlags.Static,
                    binder: null,
                    types: new[] { typeof(Dictionary<string, string[]>), typeof(string), typeof(string) },
                    modifiers: null);

                if (method is null)
                {
                    throw new InvalidOperationException("Result<T>.Validation(Dictionary<string,string[]>, string, string) not found.");
                }

                var validationResult = method.Invoke(null, new object?[] { errors, message, code });
                return (TResponse)validationResult!;
            }

            throw new InvalidOperationException($"ValidationBehavior does not support response type: {typeof(TResponse).Name}");
        }
    }
}
