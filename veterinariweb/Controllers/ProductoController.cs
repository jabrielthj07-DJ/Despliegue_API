using Capa_Entidades;
using Capa_Negocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;


namespace veterinariweb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 Requiere token válido
    public class ProductoController : ControllerBase
    {

        private readonly ProductoNegocio _productoNegocio;
        private readonly MiDbContext _context;
        public ProductoController(ProductoNegocio productoNegocio)
        {
            _productoNegocio = productoNegocio;
        }

        /// <summary>
        ///ver todos los Productos
        /// </summary>
        
        // GET: /Producto
        [HttpGet]
        [Authorize(Roles = "Administrador,Cajero")]
        public IActionResult GetProductos()
        {
            var productos = _productoNegocio.GetProductos();
            return Ok(productos);
        }

        /// <summary>
        /// Consultar Productos por ID
        /// </summary>

        // GET: /Producto/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Cajero")]
        public IActionResult GetProductoById(int id)
        {
            var producto = _productoNegocio.GetProductoById(id);
            if (producto == null)
                return NotFound($"No se encontró el producto con id {id}");

            return Ok(producto);
        }

        /// <summary>
        /// Insertar Nuevos Productos
        /// </summary>


        // POST: /Producto
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public IActionResult PostProducto([FromBody] Producto producto)
        {
            if (!ModelState.IsValid)
            {
                // Devuelve un error 400 con los mensajes de validación
                return BadRequest(ModelState);
            }

            _productoNegocio.AddProducto(producto);

            return Ok("Producto insertado correctamente");
        }

        /// <summary>
        /// Actualizar los Productos
        /// </summary> 

        // PUT: /Producto/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public IActionResult GetProductoById(int id, [FromBody] Producto producto)
        {
            if (producto == null || producto.Id_Producto != id)
                return BadRequest("Datos incorrectos para actualizar");

            _productoNegocio.UpdateProducto(producto);
            return Ok("Producto actualizado correctamente");
        }

        /// <summary>
        /// Eliminar Productos por ID
        /// </summary>

        // DELETE: /Producto/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public IActionResult DeleteProducto(int id)
        {
            try
            {
                _productoNegocio.DeleteProducto(id);
                return Ok(new { mensaje = "Producto eliminado correctamente." });
            }
            catch (SqlException ex)
            {
                // Manejo del trigger que bloquea DELETE
                if (ex.Number == 50000 && ex.Message.Contains("Eliminar registros está deshabilitado"))
                {
                    return StatusCode(403, new {
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
