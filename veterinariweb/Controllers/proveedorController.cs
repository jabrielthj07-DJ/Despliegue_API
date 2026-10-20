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
    public class proveedorController : ControllerBase
    {
        private readonly ProveedorNegocio _proveedorNegocio;
        private readonly MiDbContext _context;
        public proveedorController(ProveedorNegocio proveedorNegocio)
        {
            _proveedorNegocio = proveedorNegocio;
        }

        /// <summary>
        ///ver todos los Proveedores
        /// </summary>

        // GET: /Proveedor
        [HttpGet]
        [Authorize(Roles = "Administrador,Cajero")]
        public IActionResult GetProveedores()
        {
            var proveedor = _proveedorNegocio.GetProveedores();
            return Ok(proveedor);
        }

        /// <summary>
        /// Consultar Proveedor por ID
        /// </summary>

        // GET: /Proveedor/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Cajero")]
        public IActionResult GetProveedorById(int id)
        {
            var proveedor = _proveedorNegocio.GetProveedorById(id);
            if (proveedor == null)
                return NotFound($"No se encontró el proveedor con id {id}");

            return Ok(proveedor);
        }

        /// <summary>
        /// Insertar Nuevos Proveedores
        /// </summary>


        // POST: /Proveedor
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public IActionResult Postproveedor([FromBody] Proveedor proveedor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool resultado = _proveedorNegocio.AddProveedor(proveedor);

            if (!resultado)
            {
                return BadRequest(new
                {
                    mensaje = "El producto no existe con ese Id."
                });
            }

            return Ok(new
            {
                mensaje = "Proveedor insertado correctamente"
            });
        }

        /// <summary>
        /// Actualizar los Proveedores
        /// </summary> 

        // PUT: /Proveedor/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public IActionResult GetProveedorById(int id, [FromBody] Proveedor proveedor)
        {
            if (proveedor == null || proveedor.Id_Proveedor != id)
                return BadRequest("Datos incorrectos para actualizar");

            _proveedorNegocio.UpdateProveedor(proveedor);
            return Ok("Producto actualizado correctamente");
        }

        /// <summary>
        /// Eliminar Proveedor por ID
        /// </summary>

        // DELETE: /Producto/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public IActionResult DeleteProveedor(int id)
        {
            try
            {
                _proveedorNegocio.DeleteProveedor(id);
                return Ok(new { mensaje = "Proveedor eliminado correctamente." });
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
