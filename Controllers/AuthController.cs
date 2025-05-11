using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using TodoApi.Models.DTOs;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;

namespace TodoApi.Controllers
{
    [ApiController]
    public class AutenticacaoController : ControllerBase
    {
        private readonly TodoContext _contexto;
        private readonly IConfiguration _configuracao;
        private static Dictionary<string, int> _refreshTokens = new(); // refreshToken -> usuarioId
        public AutenticacaoController(TodoContext contexto, IConfiguration configuracao)
        {
            _contexto = contexto;
            _configuracao = configuracao;
        }

        [HttpPost("/registrar")]
        public async Task<IActionResult> Registrar(RequisicaoRegistro requisicao)
        {
            if (await _contexto.Users.AnyAsync(u => u.Email == requisicao.Email))
                return BadRequest(new { mensagem = "E-mail já cadastrado." });

            var usuario = new User
            {
                Name = requisicao.Nome,
                Email = requisicao.Email,
                PasswordHash = GerarHashSenha(requisicao.Senha),
            };
            _contexto.Users.Add(usuario);
            await _contexto.SaveChangesAsync();

            var token = GerarJwtToken(usuario);
            var refreshToken = GerarRefreshToken();
            _refreshTokens[refreshToken] = usuario.Id;
            return Ok(new RespostaAutenticacao { Token = token, RefreshToken = refreshToken });
        }

        [HttpPost("/login")]
        public async Task<IActionResult> Login(RequisicaoLogin requisicao)
        {
            var usuario = await _contexto.Users.FirstOrDefaultAsync(u => u.Email == requisicao.Email);
            if (usuario == null || usuario.PasswordHash != GerarHashSenha(requisicao.Senha))
                return Unauthorized(new { mensagem = "Credenciais inválidas." });

            var token = GerarJwtToken(usuario);
            var refreshToken = GerarRefreshToken();
            _refreshTokens[refreshToken] = usuario.Id;
            return Ok(new RespostaAutenticacao { Token = token, RefreshToken = refreshToken });
        }

        [HttpPost("/refresh")]
        public IActionResult Refresh([FromBody] RequisicaoRefreshToken requisicao)
        {
            if (!_refreshTokens.TryGetValue(requisicao.RefreshToken, out int usuarioId))
                return Unauthorized(new { mensagem = "Refresh token inválido." });

            var usuario = _contexto.Users.Find(usuarioId);
            if (usuario == null)
                return Unauthorized(new { mensagem = "Refresh token inválido." });

            var token = GerarJwtToken(usuario);
            var novoRefreshToken = GerarRefreshToken();
            _refreshTokens.Remove(requisicao.RefreshToken);
            _refreshTokens[novoRefreshToken] = usuario.Id;
            return Ok(new RespostaRefreshToken { Token = token, RefreshToken = novoRefreshToken });
        }

        private string GerarHashSenha(string senha)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha));
            return Convert.ToBase64String(bytes);
        }

        private string GerarJwtToken(User usuario)
        {
            var chaveJwt = _configuracao["Jwt:Key"] ?? "ChaveSecretaParaJwtToken";
            var emissorJwt = _configuracao["Jwt:Issuer"] ?? "EmissorTodoApi";
            var audienciaJwt = _configuracao["Jwt:Audience"] ?? "AudienciaTodoApi";
            var chaveSeguranca = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(chaveJwt));
            var credenciais = new SigningCredentials(chaveSeguranca, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var token = new JwtSecurityToken(
                issuer: emissorJwt,
                audience: audienciaJwt,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credenciais
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GerarRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
} 