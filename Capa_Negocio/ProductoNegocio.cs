using Capa_Datos;
using Capa_Entidades;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;


namespace Capa_Negocio
{
    public class ProductoNegocio
    {
        private readonly string _connectionString;
        private readonly ProductoDatos _productoDato;

        public ProductoNegocio(IConfiguration configuration)
        {
            string connectionString = (configuration.GetConnectionString("Connection") ?? configuration.GetConnectionString("DefaultConnection"));
            _productoDato = new ProductoDatos(configuration); ; // inicialización

        }


        public IEnumerable<Producto> GetProductos()
        {
            return _productoDato.GetProductos();
        }
        public Producto? GetProductoById(int id)  // mismo que se usa en el Productocontroller
        {
            return _productoDato.GetProductoById(id);
        }

        // CRUD
        public void AddProducto(Producto producto)
        {
            _productoDato.Insertar(producto);
        }

        public void UpdateProducto(Producto producto)
        {
            _productoDato.Actualizar(producto);
        }

        public void DeleteProducto(int id)
        {
            _productoDato.Eliminar(id);
        }


    }
}
