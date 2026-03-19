namespace EvalSystem.Application.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null)
        => new() { Success = true, StatusCode = 200, Data = data, Message = message ?? "Operación exitosa." };

    public static ApiResponse<T> Created(T data, string? message = null)
        => new() { Success = true, StatusCode = 201, Data = data, Message = message ?? "Recurso creado exitosamente." };

    public static ApiResponse<T> BadRequest(string message, List<string>? errors = null)
        => new() { Success = false, StatusCode = 400, Message = message, Errors = errors };

    public static ApiResponse<T> NotFound(string message)
        => new() { Success = false, StatusCode = 404, Message = message };

    public static ApiResponse<T> Unauthorized(string message = "No autorizado.")
        => new() { Success = false, StatusCode = 401, Message = message };

    public static ApiResponse<T> Forbidden(string message = "No tiene permisos para esta acción.")
        => new() { Success = false, StatusCode = 403, Message = message };

    public static ApiResponse<T> Conflict(string message)
        => new() { Success = false, StatusCode = 409, Message = message };

    public static ApiResponse<T> UnprocessableEntity(string message, List<string> errors)
        => new() { Success = false, StatusCode = 422, Message = message, Errors = errors };

    public static ApiResponse<T> InternalError(string message)
        => new() { Success = false, StatusCode = 500, Message = message };
}

public class ApiResponse : ApiResponse<object>
{
    public new static ApiResponse Ok(string? message = null)
        => new() { Success = true, StatusCode = 200, Message = message ?? "Operación exitosa." };

    public new static ApiResponse BadRequest(string message, List<string>? errors = null)
        => new() { Success = false, StatusCode = 400, Message = message, Errors = errors };

    public new static ApiResponse NotFound(string message)
        => new() { Success = false, StatusCode = 404, Message = message };

    public new static ApiResponse UnprocessableEntity(string message, List<string>? errors = null)
        => new() { Success = false, StatusCode = 422, Message = message, Errors = errors };

    public new static ApiResponse InternalError(string message)
        => new() { Success = false, StatusCode = 500, Message = message };
}
