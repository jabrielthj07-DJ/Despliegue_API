using Capa_Entidades;
using Capa_Negocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace veterinariweb.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 Requiere token válido
    public class CategoriasController : ControllerBase
    {
        private readonly CategoriaNegocio _categoriaNegocio;
        private readonly MiDbContext _context;
        public CategoriasController(CategoriaNegocio categoriaNegocio)
        {
            _categoriaNegocio = categoriaNegocio;
        }

        /// <summary>
        /// Ver Categorias disponibles
        /// </summary>

        // GET: /Producto
        [HttpGet]
        [Authorize(Roles = "Administrador,Cajero")]
        public IActionResult GetCategori()
        {
            var categorias = _categoriaNegocio.GetCategoria();
            return Ok(categorias);
        }
    }
}
