using Capa_Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace veterinariweb.Controllers
{
    /// <summary>
    /// Permite a un usuario Iniciar Sesion y generar Token JWt
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly MiDbContext _context;
        private readonly IConfiguration _config;

        public LoginController(MiDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        /// <summary>
        /// Iniciar Sesion
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                // Validar que el cuerpo JSON se haya enlazado correctamente
                if (request == null || string.IsNullOrWhiteSpace(request.Correo) || string.IsNullOrWhiteSpace(request.Clave))
                    return BadRequest(new { error = "El correo y la clave son obligatorios." });

                // Normalizar entradas
                var correoNormalizado = request.Correo.Trim();
                var claveNormalizada = request.Clave.Trim();

                // 1. Buscar usuario en la BD
                var usuario = _context.Usuarios.FirstOrDefault(u => u.Correo_Electronico == correoNormalizado);

                if (usuario == null)
                    return Unauthorized(new { mensaje = "Usuario no encontrado" });

                // 2. Validar clave
                if (usuario.Contraseña != claveNormalizada)
                    return Unauthorized(new { mensaje = "Credenciales inválidas" });

                // 3. Generar Token JWT de forma segura
                var jwtKey = _config["Jwt:Key"] ?? "ClaveSecretaSuperSeguraVeterinariaML2026!Key";
                var jwtIssuer = _config["Jwt:Issuer"] ?? "miApp";
                var jwtAudience = _config["Jwt:Audience"] ?? "miAppUsuarios";
                var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

                // Obtener el nombre del rol desde la tabla Roles
                var rol = _context.Roles
                    .Where(r => r.Id_Rol == usuario.Id_Rol)
                    .Select(r => r.cargo)
                    .FirstOrDefault() ?? "SinRol";

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, usuario.Correo_Electronico ?? string.Empty),
                    new Claim(ClaimTypes.Name, usuario.Nombre ?? string.Empty),
                    new Claim(ClaimTypes.Role, rol),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: jwtIssuer,
                    audience: jwtAudience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(8),
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256)
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                return Ok(new
                {
                    token = tokenString,
                    usuario = usuario.Nombre,
                    correo = usuario.Correo_Electronico,
                    rol_Id = usuario.Id_Rol,
                    rol = rol
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Error al procesar el inicio de sesión.",
                    detalle = ex.Message,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Permite Confirmar si el Usuario tiene Acceso Autorizado
        /// </summary>
        [Authorize]
        [HttpGet("datos")]
        public IActionResult GetDatos()
        {
            return Ok(new { mensaje = "Acceso autorizado" });
        }
    }
}
