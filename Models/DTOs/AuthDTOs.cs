using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models.DTOs
{
    public class RegisterRequest
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }

    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }

    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = null!;
    }

    public class RefreshTokenResponse
    {
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }

    public class RequisicaoRegistro
    {
        [Required]
        public string Nome { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Senha { get; set; } = null!;
    }

    public class RequisicaoLogin
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Senha { get; set; } = null!;
    }

    public class RespostaAutenticacao
    {
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }

    public class RequisicaoRefreshToken
    {
        [Required]
        public string RefreshToken { get; set; } = null!;
    }

    public class RespostaRefreshToken
    {
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }
} 