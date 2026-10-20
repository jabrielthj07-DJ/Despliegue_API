using Capa_Datos;
using Capa_Entidades;
using Microsoft.Extensions.Configuration;


namespace Capa_Negocio
{
    public class CategoriaNegocio
    {
        private readonly string _connectionString;
        private readonly CategoriaDato _categoriaDato;

        public CategoriaNegocio(IConfiguration configuration)
        {
            string connectionString = (configuration.GetConnectionString("Connection") ?? configuration.GetConnectionString("DefaultConnection"));
            _categoriaDato = new CategoriaDato(configuration); ; // inicialización

        }

        public IEnumerable<Categorias> GetCategoria()
        {
            return _categoriaDato.GetCategorias();
        }
    }
}
