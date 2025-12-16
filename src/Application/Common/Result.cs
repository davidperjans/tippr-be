namespace Application.Common
{
    public class Result
    {
        public bool IsSuccess { get; }
        public string? Error { get; }
        public IReadOnlyCollection<string> Errors { get; }

        protected Result(bool isSuccess, IEnumerable<string>? errors)
        {
            IsSuccess = isSuccess;
            var errorList = errors?.ToList() ?? new List<string>();
            Errors = errorList;
            Error = errorList.FirstOrDefault();
        }

        public static Result Success() => new(true, Array.Empty<string>());
        public static Result Failure(string error) => new(false, new[] { error });
        public static Result Failure(IEnumerable<string> errors) => new(false, errors);
    }

    public class Result<T> : Result
    {
        public T? Data { get; }

        private Result(bool isSuccess, T? data, IEnumerable<string>? errors)
            : base(isSuccess, errors)
        {
            Data = data;
        }

        public static Result<T> Success(T data) => new(true, data, Array.Empty<string>());
        public static new Result<T> Failure(string error) => new(false, default, new[] { error });
        public static new Result<T> Failure(IEnumerable<string> errors) => new(false, default, errors);
    }
}
