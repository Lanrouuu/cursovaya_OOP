namespace Cursovaya.Services;

public class ServiceResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string? WarningMessage { get; set; }

    public static ServiceResult Success()
    {
        return new ServiceResult { IsSuccess = true };
    }

    public static ServiceResult Fail(string message)
    {
        return new ServiceResult { IsSuccess = false, ErrorMessage = message };
    }
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; set; }

    public static ServiceResult<T> Success(T data, string? warningMessage = null)
    {
        return new ServiceResult<T> { IsSuccess = true, Data = data, WarningMessage = warningMessage };
    }

    public new static ServiceResult<T> Fail(string message)
    {
        return new ServiceResult<T> { IsSuccess = false, ErrorMessage = message };
    }
}
