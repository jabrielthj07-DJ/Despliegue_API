using Capa_Datos;
using Capa_Entidades;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Capa_Negocio
{
    public class ProveedorNegocio
    {
        private readonly string _connectionString;
        private readonly ProveedorDato _proveedorDato;

        public ProveedorNegocio(IConfiguration configuration)
        {
            string connectionString = (configuration.GetConnectionString("Connection") ?? configuration.GetConnectionString("DefaultConnection"));
            _proveedorDato = new ProveedorDato(configuration); ; // inicialización

        }


        public IEnumerable<Proveedor> GetProveedores()
        {
            return _proveedorDato.GetProveedores();
        }
        public Proveedor? GetProveedorById(int id)  // mismo que se usa en el Productocontroller
        {
            return _proveedorDato.GetProveedorById(id);
        }

        // CRUD
        public bool AddProveedor(Proveedor proveedor)
        {
            return _proveedorDato.Insertar(proveedor);
        }

        public void UpdateProveedor(Proveedor proveedor)
        {
            _proveedorDato.Actualizar(proveedor);
        }

        public void DeleteProveedor(int id)
        {
            _proveedorDato.Eliminar(id);
        }

    }
}
