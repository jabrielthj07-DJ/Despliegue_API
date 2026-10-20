using Capa_Entidades;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Capa_Datos
{
    public class RolDatos
    {
        private readonly string _connectionString;

        public RolDatos(IConfiguration configuration)
        {
            _connectionString = (configuration.GetConnectionString("Connection") ?? configuration.GetConnectionString("DefaultConnection"));
        }

        // Probar conexión
        public void TestConnection()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                Console.WriteLine("Conexión exitosa");
            }
        }

        // Obtener todos los productos
        public IEnumerable<Rol> GetRol()
        {
            var rol = new List<Rol>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id_Rol, cargo FROM Roles";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rol.Add(new Rol
                        {
                            Id_Rol = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                            cargo = reader.IsDBNull(1) ? null : reader.GetString(1)
                        });
                    }
                }
            }

            return rol;
        }
    }
}
