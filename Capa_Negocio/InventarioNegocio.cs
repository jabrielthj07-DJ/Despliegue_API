using Capa_Datos;
using Microsoft.Extensions.Configuration;

namespace Capa_Negocio
{
    public class InventarioNegocio
    {
        private readonly string _connectionString;
        private readonly InventarioDato _inventarioDato;

        public InventarioNegocio(IConfiguration configuration)
        {
            string connectionString = (configuration.GetConnectionString("Connection") ?? configuration.GetConnectionString("DefaultConnection"));
            _inventarioDato = new InventarioDato(configuration); ; // inicialización

        }

        // Método para insertar inventario con stock y precio
        public string InsertarInventario(int idProducto, string descripcion, int stock, decimal precio)
        {
            return _inventarioDato.InsertarInventario(idProducto, descripcion, stock, precio);
        }


        // Método para actualizar inventario con stock y precio
        public string ActualizarInventario(int idInventario, int idProducto, string descripcion, int stock, decimal precio)
        {
            return _inventarioDato.ActualizarInventario(idInventario, idProducto, descripcion, stock, precio);
        }


    }
}
