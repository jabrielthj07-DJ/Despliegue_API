using Capa_Entidades;
using Capa_Negocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace veterinariweb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 Requiere token válido
    public class ComprasController : ControllerBase
    {
        private readonly Compra_Negocio _compraNegocio;
        private readonly MiDbContext _context;

        public ComprasController(Compra_Negocio compra_Negocio)
        {
            _compraNegocio = compra_Negocio;
        }

        /// <summary>
        /// Consultar todas las compras realizadas
        /// </summary>

        // GET
        // Obtiene todas las compras con sus detalles
        [HttpGet]
        [Authorize(Roles = "Administrador,Cajero")]
        public IActionResult GetComprasConDetalles()
        {
            try
            {
                var compras = _compraNegocio.GetComprasConDetalles();

                if (compras == null || !compras.Any())
                    return NotFound("No se encontraron compras registradas.");

                return Ok(compras);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener las compras: {ex.Message}");
            }
        }

        /// <summary>
        /// Consultar una compra por ID
        /// </summary>

        // GET
        // Obtiene una compra con sus detalles por Id
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Cajero")]
        public IActionResult GetCompraPorId(int id)
        {
            try
            {
                var compra = _compraNegocio.ObtenerCompraPorId(id);

                if (compra == null)
                    return NotFound($"No se encontró la compra con Id {id}.");

                return Ok(compra);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener la compra por Id: {ex.Message}");
            }
        }

        /// <summary>
        /// Registrar una compra
        /// </summary>

        [HttpPost]
        [Authorize(Roles = "Administrador,Cajero")]

        // POST
        public IActionResult PostCompra([FromBody] Compras compras)
        {
            try
            {
                _compraNegocio.InsertarCompra(compras);

                return Ok("Compra registrada correctamente");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Eliminar compra
        /// </summary>

        // DELETE api/compras/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public IActionResult DeleteCompra(int id)
        {
            try
            {
                var compraExistente = _compraNegocio.ObtenerCompraPorId(id);

                if (compraExistente == null)
                    return NotFound(new
                    {
                        mensaje = $"No se encontró la compra con id {id}"
                    });

                _compraNegocio.EliminarCompra(id);

                return Ok(new
                {
                    mensaje = $"Compra con Id {id} eliminada correctamente."
                });
            }
            catch (SqlException ex)
            {
                // Manejo de triggers que bloquean DELETE
                if (ex.Number == 50000 &&
                    ex.Message.Contains("Eliminar registros está deshabilitado"))
                {
                    return StatusCode(403, new
                    {
                        mensaje = "No hay acceso para eliminar la compra, por ética no es recomendable desechar datos almacenados. " +
                                   "Si realmente desea eliminar, consulte con el dueño del local o soporte técnico de confianza."
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        mensaje = ex.Message
                    });
                }
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(new
                {
                    mensaje = knfEx.Message
                });
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new
                {
                    mensaje = argEx.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = ex.Message
                });
            }
        }
    }
}
