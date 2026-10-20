using Capa_Datos;
using Capa_Entidades;
using Microsoft.Extensions.Configuration;

namespace Capa_Negocio
{
    public class Rol_Negocio
    {
        private readonly string _connectionString;
        private readonly RolDatos _rolDato;

        public Rol_Negocio(IConfiguration configuration)
        {
            string connectionString = (configuration.GetConnectionString("Connection") ?? configuration.GetConnectionString("DefaultConnection"));
            _rolDato = new RolDatos(configuration); ; // inicialización

        }

        public IEnumerable<Rol> GetRol()
        {
            return _rolDato.GetRol();
        }
    }
}
