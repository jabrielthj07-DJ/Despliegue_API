using Capa_Entidades; 
using Capa_Negocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Linq;
using System;
using Capa_Datos;


namespace veterinariweb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 🔒 Requiere token válido
    public class VentasController : ControllerBase
    {
        private readonly Venta_Negocio _ventaNegocio;
        private readonly MiDbContext _context;
        public VentasController(Venta_Negocio venta_Negocio)
        {
            _ventaNegocio = venta_Negocio;
        }

        /// <summary>
        /// Consultar todas las Ventas realizadas
        /// </summary>


        //  GET
        // Obtiene todas las ventas con sus detalles
        [HttpGet]
        [Authorize(Roles = "Administrador,Cajero")]
        public IActionResult GetVentasConDetalles()
        {
            try
            {
                var ventas = _ventaNegocio.GetVentasConDetalles();

                if (ventas == null || !ventas.Any())
                    return NotFound("No se encontraron ventas registradas.");

                return Ok(ventas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener las ventas: {ex.Message}");
            }
        }

        /// <summary>
        /// Consultar una venta por ID
        /// </summary>
        /// <returns>token valido</returns>
        /// 

        //  GET
        // Obtiene una venta con sus detalles por Id
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Cajero")]
        public IActionResult GetVentasConDetalles(int id)
        {
            try
            {
                var ventas = _ventaNegocio.GetVentasConDetalles();

                if (ventas == null)
                    return NotFound($"No se encontró la venta con Id {id}.");
                return Ok(ventas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener las ventas por Id: {ex.Message}");
            }
        }
        /// <summary>
        /// Realizar una Venta
        /// </summary>

        [HttpPost]
        [Authorize(Roles = "Administrador,Cajero")]
        //POST "Registro"
        public IActionResult PostVenta([FromBody] Ventas ventas)
        {
            try
            {
                _ventaNegocio.InsertarVenta(ventas);
                return Ok("Venta registrada correctamente");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Eliminar Ventas
        /// </summary>

        // DELETE api/ventas/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public IActionResult DeleteVenta(int id)
        {
            try
            {
                var ventaExistente = _ventaNegocio.ObtenerVentaPorId(id);
                if (ventaExistente == null)
                    return NotFound(new { mensaje = $"No se encontró la venta con id {id}" });

                _ventaNegocio.EliminarVenta(id); // Llamamos al método de la capa de negocio
                return Ok(new { mensaje = $"Venta con Id {id} eliminada correctamente." });
            }
            catch (SqlException ex)
            {
                // Manejo de triggers que bloquean DELETE
                if (ex.Number == 50000 && ex.Message.Contains("Eliminar registros está deshabilitado"))
                {
                    return StatusCode(403, new
                    {
                        mensaje = "No hay acceso para eliminar la venta, por ética no es recomendable desechar datos almacenados. " +
                                  "Si realmente desea eliminar, consulte con el dueño del local o soporte técnico de confianza."
                    });
                }
                else
                {
                    return StatusCode(500, new { mensaje = "" + ex.Message });
                }
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(new { mensaje = knfEx.Message });
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(new { mensaje = argEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "" + ex.Message });
            }
        }



    }
}
