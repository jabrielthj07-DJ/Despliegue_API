using Capa_Entidades;
using Capa_Negocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using veterinariweb.Context;
using static Capa_Entidades.Usuario;


namespace veterinariweb.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 Requiere token válido
    public class UsuarioRolController : ControllerBase
    {
        private readonly UsuarioNegocio _usuarioNegocio;
        private readonly AppDbContext _context;
        public UsuarioRolController(UsuarioNegocio usuarioNegocio)
        {
            _usuarioNegocio = usuarioNegocio;
        }

        /// <summary>
        /// Ver todos los usuarios
        /// </summary>
 
        // GET: /Usuario
        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public IActionResult GetUsuarios()
        {
            var usuarios = _usuarioNegocio.GetUsuarios();
            return Ok(usuarios);
        }
        /// <summary>
        /// Consultar Usuarios por ID
        /// </summary>

        //Get por Id
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador")]
        public IActionResult GetUsuarioById(int id)
        {
            var usuario = _usuarioNegocio.GetUsuarioById(id);
            if (usuario == null)
                return NotFound($"No se encontró el usuario con id {id}");

            return Ok(usuario);
        }

        /// <summary>
        /// Crear nuevos Usuarios
        /// </summary>

        // POST api/usuario

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public IActionResult CrearUsuario([FromBody] UsuarioCreateDto usuarioDto)
        {
            try
            {
                // Crear objeto Usuario a partir del DTO
                var usuario = new Usuario
                {
                    Nombre = usuarioDto.Nombre,
                    Correo_Electronico = usuarioDto.Correo_Electronico,
                    Contraseña = usuarioDto.Contraseña, // contraseña en plano
                    Id_Rol = usuarioDto.Id_Rol
                };

                // 🔑 Pasar la contraseña como segundo parámetro
                var usuarioCreado = _usuarioNegocio.Insertar(usuario, usuario.Contraseña);

                if (usuarioCreado == null)
                    return BadRequest(new { mensaje = "No se pudo insertar el usuario." });

                // Retornar solo los datos necesarios (sin contraseña)
                return Ok(new
                {
                    mensaje = "Usuario insertado correctamente.",
                    usuario = usuarioCreado
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error en el servidor: " + ex.Message });
            }
        }

        /// <summary>
        /// Atualizar Usuarios
        /// </summary>
        
        // PUT: Actualizar usuario
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public IActionResult ActualizarUsuario(int id, [FromBody] UsuarioUpdateDto usuarioDto)
        {
            var usuarioExistente = _usuarioNegocio.GetUsuarioById(id);
            if (usuarioExistente == null)
                return NotFound($"No se encontró el usuario con id {id}");

            // Solo actualizamos los campos que vengan
            if (!string.IsNullOrEmpty(usuarioDto.Nombre))
                usuarioExistente.Nombre = usuarioDto.Nombre;

            if (!string.IsNullOrEmpty(usuarioDto.Correo_Electronico))
                usuarioExistente.Correo_Electronico = usuarioDto.Correo_Electronico;

            if (!string.IsNullOrEmpty(usuarioDto.Contraseña))
                _usuarioNegocio.UpdateUsuario(usuarioExistente, usuarioDto.Contraseña);
            else
                _usuarioNegocio.UpdateUsuario(usuarioExistente);

            if (usuarioDto.RolId.HasValue)
                usuarioExistente.Id_Rol = usuarioDto.RolId.Value;

            return Ok(new { mensaje = "Usuario actualizado correctamente." });
        }
        
        /// <summary>
        /// Eliminar Usuarios por ID
        /// </summary>

        // DELETE: Eliminar usuario
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public IActionResult EliminarUsuario(int id)
        {
            try
            {
                var usuarioExistente = _usuarioNegocio.GetUsuarioById(id);
                if (usuarioExistente == null)
                    return NotFound(new { mensaje = $"No se encontró el usuario con id {id}" });

                _usuarioNegocio.DeleteUsuario(id);
                return Ok(new { mensaje = "Usuario eliminado correctamente." });
            }
            catch (SqlException ex)
            {
                // Manejo del trigger que bloquea DELETE
                if (ex.Number == 50000 && ex.Message.Contains("Eliminar registros está deshabilitado"))
                {
                    return StatusCode(403, new
                    {
                        mensaje = "No hay acceso para eliminar, por ética no es recomendable desechar datos almacenados. " +
                                  "Si realmente desea eliminar, consulte con el dueño del local o soporte técnico de confianza"
                    });
                }
                else
                {
                    return StatusCode(500, "Error interno del servidor: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor: " + ex.Message);
            }
        }
    }

}

