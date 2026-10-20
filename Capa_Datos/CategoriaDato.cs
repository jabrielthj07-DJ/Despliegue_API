using Capa_Entidades;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Capa_Datos
{
    public class CategoriaDato
    {
        private readonly string _connectionString;

        public CategoriaDato(IConfiguration configuration)
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
        public IEnumerable<Categorias> GetCategorias()
        {
            var productos = new List<Categorias>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id_Categoria, Nombre_Categoria, Descripcion FROM Categorias";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        productos.Add(new Categorias
                        {
                            Id_Categoria = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                            Nombre_Categoria = reader.IsDBNull(1) ? null : reader.GetString(1),
                            Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
                            
                        });
                    }
                }
            }

            return productos;
        }

    }
}
