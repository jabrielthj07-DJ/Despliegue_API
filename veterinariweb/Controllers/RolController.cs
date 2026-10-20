using Capa_Entidades;
using Capa_Negocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace veterinariweb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 Requiere token válido
    public class RolController : ControllerBase
    {
        private readonly Rol_Negocio _rolNegocio;
        private readonly MiDbContext _context;
        public RolController(Rol_Negocio rol_Negocio)
        {
            _rolNegocio = rol_Negocio;
        }

        /// <summary>
        /// Se muestra los Roles disponibles para crear nuevos Usuarios
        /// </summary>


        // GET: /Producto
        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public IActionResult GetRol()
        {
            var rol= _rolNegocio.GetRol();
            return Ok(rol);
        }
    }
}
