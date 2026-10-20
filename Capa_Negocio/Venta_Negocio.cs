using Capa_Datos;
using Capa_Entidades;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Capa_Negocio
{
    public class Venta_Negocio
    {
        private readonly string _connectionString;
        private readonly VentaDatos _VentaDatos;

        public Venta_Negocio(IConfiguration configuration)
        {
            string connectionString = (configuration.GetConnectionString("Connection") ?? configuration.GetConnectionString("DefaultConnection"));
            _VentaDatos = new VentaDatos(configuration); ; // inicialización

        }

        public IEnumerable<Ventas> GetVentasConDetalles()
        {
            return _VentaDatos.GetVentasConDetalles();
        }

        /// <summary>
        /// Obtiene una venta con sus detalles por Id
        /// </summary>
        public Ventas ObtenerVentaPorId(int idVenta)
        {
            if (idVenta <= 0)
                throw new ArgumentException("El Id de la venta debe ser mayor a cero.");

            var venta = _VentaDatos.GetVentaPorId(idVenta);

            if (venta == null)
                throw new KeyNotFoundException("No se encontró la venta con el Id proporcionado.");

            return venta;
        }

        public void InsertarVenta(Ventas ventas) 
        { 
            /*Utilizamos el || para validar si dos condiciones se cumple
            *! con este valor negamos la condicion
            *Any si la condicion se cumple
            */
            if (ventas == null || !ventas.Detalles.Any())
            {
                //throw ArgumentException argumentamos la excepcion "xyz"
                throw new ArgumentNullException("La venta");
            }
            //Validar && si la condicion se cumple 
            //if (venta.TipoPago == "Tarjeta " && venta.pago < 100)

            foreach (var detalles in ventas.Detalles) 
            {
                if (detalles.Cantidad <= 0)
                {
                    throw new ArgumentException("La cantidad debe ser mayor");
                }
                //validar el precio
                ventas.Fecha_Venta=DateTime.Now;
                ventas.Total = ventas.Detalles.Sum(d => d.Cantidad * d.Precio_Unitario);
                _VentaDatos.InsertarVenta(ventas);
            }
        }

        public void EliminarVenta(int idVenta)
        {
            if (idVenta <= 0)
                throw new ArgumentException("El Id de la venta debe ser mayor a cero.");

            // Verificar si la venta existe
            var ventaExistente = _VentaDatos.GetVentaPorId(idVenta);
            if (ventaExistente == null)
                throw new KeyNotFoundException("No se encontró la venta con el Id proporcionado.");

            // Llamamos a la capa de datos para eliminar
            _VentaDatos.EliminaVenta(idVenta);
        }


    }
}
