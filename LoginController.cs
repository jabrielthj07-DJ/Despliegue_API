csharp veterinariweb\Controllers\LoginController.cs
using Capa_Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace veterinariweb.Controllers
{
    /// <summary>
    /// Permite a un usuario Iniciar Sesion y generar Token JWt
    /// </summary>
    /// <returns>token valido</returns>
    ///     
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
            // Validar que el cuerpo JSON se haya enlazado correctamente
            if (request == null)
                return BadRequest(new { error = "Cuerpo JSON inválido o malformado." });

            // Normalizar entradas
            request.Correo = request.Correo?.Trim();
            request.Clave = request.Clave?.Trim();

            //  1. Buscar usuario en la BD
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Correo_Electronico == request.Correo);

            if (usuario == null)
                return Unauthorized("Usuario no encontrado");

            //  2. Validar clave
            if (usuario.Contraseña != request.Clave)
                return Unauthorized("Credenciales inválidas");

            //  3. Generar Token
            var jwtSettings = _config.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

            // Obtener el nombre del rol desde la tabla Roles
            var rol = _context.Roles
             .Where(r => r.Id_Rol == usuario.Id_Rol)
             .Select(r => r.cargo)
             .FirstOrDefault() ?? "SinRol";

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Correo_Electronico),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Role, rol),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                Token = tokenString,
                Usuario = usuario.Nombre,
                Correo = usuario.Correo_Electronico,
                Rol_Id = usuario.Id_Rol,
                Rol = rol
            });
        }

        /// <summary>
        /// Permite Confirmar si el Usuario tiene Acceso Autorizado
        /// </summary>
        /// <returns>token valido</returns>
        [Authorize]
        [HttpGet("datos")]
        public IActionResult GetDatos()
        {
            return Ok("Acceso autorizado");
        }
    }
}
