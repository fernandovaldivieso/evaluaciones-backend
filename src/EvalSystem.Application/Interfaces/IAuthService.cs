using EvalSystem.Application.Common;
using EvalSystem.Application.DTOs.Auth;

namespace EvalSystem.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<ApiResponse> LogoutAsync(string refreshToken);
}
