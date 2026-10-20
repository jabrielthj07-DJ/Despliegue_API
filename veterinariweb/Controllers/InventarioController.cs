using Capa_Entidades;
using Capa_Negocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace veterinariweb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 Requiere token válido
    public class InventarioController : ControllerBase
    {
        private readonly InventarioNegocio _inventarioNegocio;
        private readonly MiDbContext _context;
        public InventarioController(InventarioNegocio inventarioNegocio)
        {
            _inventarioNegocio = inventarioNegocio;
        }
        /// <summary>
        /// Consultar Inventario actual
        /// </summary>
        /// <returns>token valido</returns>

        // Método para obtener todos los inventarios (opcional para GET en Swagger)
        [HttpGet]
        public IActionResult GetInventarios([FromServices] MiDbContext context)
        {
            var inventarios = context.Inventarios.ToList(); // Asegúrate de tener DbSet<Inventario> en tu DbContext
            return Ok(inventarios);
        }

        /// <summary>
        /// Buscar Inventario por ID del Producto
        /// </summary>
        /// 
        // Método para obtener inventario por Id_Producto
        [HttpGet("{idProducto}")]
        public IActionResult GetInventarioPorProducto(int idProducto, [FromServices] MiDbContext context)
        {
            try
            {
                var inventario = context.Inventarios
                                        .FirstOrDefault(i => i.Id_Producto == idProducto); // Buscar por Id_Producto

                if (inventario == null)
                    return NotFound(new { mensaje = "No se encontró inventario para el producto especificado." });

                return Ok(inventario);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Insertar Nuevo Producto en el Inventario
        /// </summary>
        /// <returns>token valido</returns>
        // Método para insertar inventario
        [HttpPost]
        public IActionResult InsertarInventario([FromBody] Inventario inventario, [FromServices] MiDbContext context)
        {
            try
            {
                // Validar si ya existe un inventario con el mismo Id_Producto
                var inventarioExistente = context.Inventarios
                                                 .FirstOrDefault(i => i.Id_Producto == inventario.Id_Producto);

                if (inventarioExistente != null)
                {
                    return BadRequest(new { error = "Ya existe un inventario con ese Id_Producto." });
                }

                // Llamar a tu método de negocio para insertar
                var resultado = _inventarioNegocio.InsertarInventario(
                    inventario.Id_Producto,
                    inventario.Descripcion,
                    inventario.Stock,
                    inventario.Precio
                );

                return Ok(new { mensaje = resultado });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Actualizar Inventario
        /// </summary>
        /// <returns>token valido</returns>
        
        // Método para actualizar inventario
        [HttpPut]
        public IActionResult ActualizarInventario([FromBody] Inventario inventario)
        {
            try
            {
                var resultado = _inventarioNegocio.ActualizarInventario(
                    inventario.Id_Inventario,
                    inventario.Id_Producto,
                    inventario.Descripcion,
                    inventario.Stock,
                    inventario.Precio
                );

                return Ok(new { mensaje = resultado });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

    }

}

