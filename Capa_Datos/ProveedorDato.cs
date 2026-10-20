using Capa_Entidades;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Capa_Datos
{
    public class ProveedorDato
    {
        private readonly string _connectionString;

        public ProveedorDato(IConfiguration configuration)
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

        // Obtener todos los proveedores
        public IEnumerable<Proveedor> GetProveedores()
        {
            var proveedores = new List<Proveedor>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"
            SELECT 
                Id_Proveedor,
                Nombre,
                Apellido,
                Email,
                Telefono,
                Razon_Social,
                Direccion,
                Id_Producto
            FROM Proveedor";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        proveedores.Add(new Proveedor
                        {
                            Id_Proveedor = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                            Nombre = reader.IsDBNull(1) ? null : reader.GetString(1),
                            Apellido = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Telefono = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Razon_Social = reader.IsDBNull(5) ? null : reader.GetString(5),
                            Direccion = reader.IsDBNull(6) ? null : reader.GetString(6),
                            Id_Producto = reader.IsDBNull(7) ? 0 : reader.GetInt32(7)
                        });
                    }
                }
            }

            return proveedores;
        }

        // Obtener proveedor por Id
        public Proveedor? GetProveedorById(int Id_Proveedor)
        {
            Proveedor? proveedor = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"
            SELECT 
                Id_Proveedor,
                Nombre,
                Apellido,
                Email,
                Telefono,
                Razon_Social,
                Direccion,
                Id_Producto
            FROM Proveedor
            WHERE Id_Proveedor = @Id_Proveedor";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Id_Proveedor", Id_Proveedor);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            proveedor = new Proveedor
                            {
                                Id_Proveedor = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                Nombre = reader.IsDBNull(1) ? null : reader.GetString(1),
                                Apellido = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Telefono = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Razon_Social = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Direccion = reader.IsDBNull(6) ? null : reader.GetString(6),
                                Id_Producto = reader.IsDBNull(7) ? 0 : reader.GetInt32(7)
                            };
                        }
                    }
                }
            }

            return proveedor;
        }

        // Insertar proveedor
        public bool Insertar(Proveedor proveedor)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Validar si existe el producto
                string validarProducto = @"
            SELECT COUNT(*) 
            FROM Producto 
            WHERE Id_Producto = @Id_Producto";

                using (SqlCommand validarCmd = new SqlCommand(validarProducto, connection))
                {
                    validarCmd.Parameters.AddWithValue("@Id_Producto", proveedor.Id_Producto);

                    int existe = (int)validarCmd.ExecuteScalar();

                    if (existe == 0)
                    {
                        return false;
                    }
                }

                string query = @"
            INSERT INTO Proveedor
            (
                Nombre,
                Apellido,
                Email,
                Telefono,
                Razon_Social,
                Direccion,
                Id_Producto
            )
            VALUES
            (
                @Nombre,
                @Apellido,
                @Email,
                @Telefono,
                @Razon_Social,
                @Direccion,
                @Id_Producto
            )";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Nombre", proveedor.Nombre);
                    cmd.Parameters.AddWithValue("@Apellido", proveedor.Apellido);
                    cmd.Parameters.AddWithValue("@Email", proveedor.Email);
                    cmd.Parameters.AddWithValue("@Telefono", proveedor.Telefono);
                    cmd.Parameters.AddWithValue("@Razon_Social", proveedor.Razon_Social);
                    cmd.Parameters.AddWithValue("@Direccion", proveedor.Direccion);
                    cmd.Parameters.AddWithValue("@Id_Producto", proveedor.Id_Producto);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // Actualizar proveedor
        public void Actualizar(Proveedor proveedor)
        {
            using SqlConnection con = new(_connectionString);

            con.Open();

            using SqlCommand cmd = new("sp_ActualizarProveedor", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Id_Proveedor", proveedor.Id_Proveedor);
            cmd.Parameters.AddWithValue("@Nombre", proveedor.Nombre);
            cmd.Parameters.AddWithValue("@Apellido", proveedor.Apellido);
            cmd.Parameters.AddWithValue("@Email", proveedor.Email);
            cmd.Parameters.AddWithValue("@Telefono", proveedor.Telefono);
            cmd.Parameters.AddWithValue("@Razon_Social", proveedor.Razon_Social);
            cmd.Parameters.AddWithValue("@Direccion", proveedor.Direccion);
            cmd.Parameters.AddWithValue("@Id_Producto", proveedor.Id_Producto);

            cmd.ExecuteNonQuery();

            con.Close();
        }

        // Eliminar proveedor
        public void Eliminar(int Id_Proveedor)
        {
            try
            {
                using SqlConnection con = new(_connectionString);

                con.Open();

                using SqlCommand cmd = new("sp_EliminarProveedor", con)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@Id_Proveedor", Id_Proveedor);

                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al eliminar proveedor: {ex.Message}");
                throw;
            }
        }
    }
}
