namespace Shared.Model;

public class OperationResult<T>
{
    private T Data { get; set; }
    public string? ErrorMessageCode { get; set; } = null!;
    public bool IsSuccess => string.IsNullOrEmpty(ErrorMessageCode);

    public OperationResult(string errorMessageCode)
    {
        ErrorMessageCode = errorMessageCode;
    }

    public OperationResult(T obj)
    {
        Data = obj;
    }

    public bool TryGetValue(out T obj)
    {
        obj = default!;
        if (IsSuccess)
        {
            obj = Data;
            return true;
        }

        return false;
    }
}

public class OperationResult
{
    public string ErrorMessageCode { get; set; } = null!;
    public bool IsSuccess => string.IsNullOrEmpty(ErrorMessageCode);
    public OperationResult(string errorMessageCode)
    {
        ErrorMessageCode = errorMessageCode;
    }
    private OperationResult()
    {
        
    }
    public static OperationResult Success()
    {
        return new OperationResult();
    }
}