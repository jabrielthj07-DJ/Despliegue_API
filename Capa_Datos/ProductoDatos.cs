using Capa_Entidades;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Capa_Datos
{
    public class ProductoDatos
    {
        private readonly string _connectionString;

        public ProductoDatos(IConfiguration configuration)
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
        public IEnumerable<Producto> GetProductos()
        {
            var productos = new List<Producto>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id_Producto, Nombre_Producto, Descripcion, Id_Categoria FROM Producto";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        productos.Add(new Producto
                        {
                            Id_Producto = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                            Nombre_Producto = reader.IsDBNull(1) ? null : reader.GetString(1),
                            Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Id_Categoria = reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
                        });
                    }
                }
            }

            return productos;
        }

        // Obtener producto por Id
        public Producto? GetProductoById(int Id_Producto)
        {
            Producto? producto = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Id_Producto, Nombre_Producto, Descripcion, Id_Categoria FROM Producto WHERE Id_Producto = @Id_Producto";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Id_Producto", Id_Producto);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            producto = new Producto
                            {
                                Id_Producto = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                Nombre_Producto = reader.IsDBNull(1) ? null : reader.GetString(1),
                                Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Id_Categoria = reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
                            };
                        }
                    }
                }
            }

            return producto;
        }

        // Insertar producto
        public bool Insertar(Producto producto)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                INSERT INTO Producto (Nombre_Producto, Descripcion, Id_Categoria) 
                VALUES (@Nombre_Producto, @Descripcion, @Id_Categoria)";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Nombre_Producto", producto.Nombre_Producto);
                    cmd.Parameters.AddWithValue("@Descripcion", producto.Descripcion);
                    cmd.Parameters.AddWithValue("@Id_Categoria", producto.Id_Categoria);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // Actualizar producto (con SP)
        public void Actualizar(Producto producto)
        {
            using SqlConnection con = new(_connectionString);
            con.Open();
            using SqlCommand cmd = new("sp_ActualizarProducto", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Id_Producto", producto.Id_Producto);
            cmd.Parameters.AddWithValue("@Nombre_Producto", producto.Nombre_Producto);
            cmd.Parameters.AddWithValue("@Descripcion", producto.Descripcion);
            cmd.Parameters.AddWithValue("@Id_Categoria", producto.Id_Categoria);

            cmd.ExecuteNonQuery();
            con.Close();
        }

        // Eliminar producto (con SP)
        public void Eliminar(int Id_Producto)
        {
            try
            {
                using SqlConnection con = new(_connectionString);
                con.Open();
                using SqlCommand cmd = new("sp_EliminarProducto", con)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@Id_Producto", Id_Producto);
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al eliminar producto: {ex.Message}");
                throw;
            }
        }
    }
}

