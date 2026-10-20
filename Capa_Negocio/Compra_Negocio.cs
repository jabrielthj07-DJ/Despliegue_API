using Capa_Datos;
using Capa_Entidades;
using Microsoft.Extensions.Configuration;
using System;

namespace Capa_Negocio
{
    public class Compra_Negocio
    {
        private readonly string _connectionString;
        private readonly CompraDato _CompraDatos;

        public Compra_Negocio(IConfiguration configuration)
        {
            string connectionString = (configuration.GetConnectionString("Connection") ?? configuration.GetConnectionString("DefaultConnection"));

            _CompraDatos = new CompraDato(configuration);
        }

        // Obtener todas las compras con detalles
        public IEnumerable<Compras> GetComprasConDetalles()
        {
            return _CompraDatos.GetComprasConDetalles();
        }

        /// <summary>
        /// Obtener una compra por Id con detalles
        /// </summary>
        public Compras ObtenerCompraPorId(int idCompra)
        {
            if (idCompra <= 0)
                throw new ArgumentException("El Id de la compra debe ser mayor a cero.");

            var compra = _CompraDatos.GetCompraPorId(idCompra);

            if (compra == null)
                throw new KeyNotFoundException("No se encontró la compra con el Id proporcionado.");

            return compra;
        }

        // Insertar compra
        public void InsertarCompra(Compras compras)
        {
            // Validar compra vacía
            if (compras == null || !compras.Detalles.Any())
            {
                throw new ArgumentNullException("La compra");
            }

            // Validar detalles
            foreach (var detalle in compras.Detalles)
            {
                // Validar cantidad
                if (detalle.Cantidad <= 0)
                {
                    throw new ArgumentException("La cantidad debe ser mayor a cero.");
                }

                // Validar precio
                if (detalle.Precio_Unitario <= 0)
                {
                    throw new ArgumentException("El precio debe ser mayor a cero.");
                }
            }

            // Asignar fecha automática
            compras.Fecha_Compra = DateTime.Now;

            // Calcular total automáticamente
            compras.Total = compras.Detalles.Sum(d => d.Cantidad * d.Precio_Unitario);

            // Insertar en capa datos
            _CompraDatos.InsertarCompra(compras);
        }

        // Eliminar compra
        public void EliminarCompra(int idCompra)
        {
            if (idCompra <= 0)
                throw new ArgumentException("El Id de la compra debe ser mayor a cero.");

            // Verificar si existe
            var compraExistente = _CompraDatos.GetCompraPorId(idCompra);

            if (compraExistente == null)
                throw new KeyNotFoundException("No se encontró la compra con el Id proporcionado.");

            // Eliminar
            _CompraDatos.EliminarCompra(idCompra);
        }
    }
}
