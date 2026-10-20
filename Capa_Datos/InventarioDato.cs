using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Capa_Datos
{
    public class InventarioDato
    {
        private readonly string _connectionString;

        public InventarioDato(IConfiguration configuration)
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

        // Método para insertar Inventario
        public string InsertarInventario(int idProducto, string descripcion, int? stock = null, decimal? precio = null)
        {
            string rpta = "";
            try
            {
                using (SqlConnection cn = new SqlConnection(_connectionString))
                {
                    cn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_InsertarInventario", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Id_Producto", idProducto);
                        cmd.Parameters.AddWithValue("@Descripcion", descripcion);
                        cmd.Parameters.AddWithValue("@Stock", stock.HasValue ? (object)stock.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@Precio", precio.HasValue ? (object)precio.Value : DBNull.Value);

                        int filasAfectadas = cmd.ExecuteNonQuery();

                        return "Inventario insertado correctamente";
                    }
                }
            }
            catch (Exception ex)
            {
                rpta = ex.Message;
            }
            return rpta;
        }


        // Método para actualizar Inventario
        public string ActualizarInventario(int idInventario, int idProducto, string descripcion, int? stock = null, decimal? precio = null)
        {
            string rpta = "";
            try
            {
                using (SqlConnection cn = new SqlConnection(_connectionString))
                {
                    cn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_ActualizarInventario", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Id_Inventario", idInventario);
                        cmd.Parameters.AddWithValue("@Id_Producto", idProducto);
                        cmd.Parameters.AddWithValue("@Descripcion", descripcion);

                        cmd.Parameters.AddWithValue("@Stock", stock.HasValue ? (object)stock.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@Precio", precio.HasValue ? (object)precio.Value : DBNull.Value);

                        int filasAfectadas = cmd.ExecuteNonQuery();
                        return "Inventario actualizado correctamente"; 
                    }
                }
            }
            catch (Exception ex)
            {
                rpta = ex.Message;
            }
            return rpta;
        }
    }
}
