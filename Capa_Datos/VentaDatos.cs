using Capa_Entidades;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Capa_Datos
{
    public class VentaDatos
    {
        //declaracion de la cadena de conecxion con readonly
        private readonly string _connectionString;

        public VentaDatos(IConfiguration configuration)
        {
            _connectionString = (configuration.GetConnectionString("Connection") ?? configuration.GetConnectionString("DefaultConnection"));
        }

        // Conexión
        public void TestConnection()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                Console.WriteLine("Conexión exitosa");
            }
        }

        //obtener ventas
        public IEnumerable<Ventas> GetVentasConDetalles()
        {
            var ventas = new List<Ventas>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // 1️ Obtener todas las ventas
                string queryVentas = "SELECT Id_Venta, Id_Usuario, Fecha_Venta, Total, Nombre FROM Ventas";

                using (SqlCommand cmdVentas = new SqlCommand(queryVentas, connection))
                using (SqlDataReader readerVentas = cmdVentas.ExecuteReader())
                {
                    while (readerVentas.Read())
                    {
                        var venta = new Ventas
                        {
                            Id_Venta = readerVentas.IsDBNull(0) ? 0 : readerVentas.GetInt32(0),
                            Id_Usuario = readerVentas.IsDBNull(1) ? 0 : readerVentas.GetInt32(1),
                            Fecha_Venta = readerVentas.IsDBNull(2) ? DateTime.MinValue : readerVentas.GetDateTime(2),
                            Total = readerVentas.IsDBNull(3) ? 0 : readerVentas.GetDecimal(3),
                            Nombre = readerVentas.IsDBNull(4) ? null : readerVentas.GetString(4),
                            Detalles = new List<DetalleVenta>()
                        };

                        ventas.Add(venta);
                    }
                }

                // 2️ Obtener los detalles para cada venta
                foreach (var venta in ventas)
                {
                    string queryDetalles = @"
                      SELECT Id_Detalle_Venta, Id_Producto, Nombre_Producto, Cantidad, Precio_Unitario
                      FROM Detalle_Venta
                      WHERE Id_Venta = @Id_Venta";


                    using (SqlCommand cmdDetalles = new SqlCommand(queryDetalles, connection))
                    {
                        cmdDetalles.Parameters.AddWithValue("@Id_Venta", venta.Id_Venta);

                        using (SqlDataReader readerDetalles = cmdDetalles.ExecuteReader())
                        {
                            while (readerDetalles.Read())
                            {
                                var detalle = new DetalleVenta
                                {
                                    Id_Venta = venta.Id_Venta,
                                    Id_Detalle_Venta = readerDetalles.IsDBNull(0) ? 0 : readerDetalles.GetInt32(0),
                                    Id_Producto = readerDetalles.IsDBNull(1) ? 0 : readerDetalles.GetInt32(1),
                                    Nombre_Producto = readerDetalles.IsDBNull(2) ? null : readerDetalles.GetString(2), // <-- aquí
                                    Cantidad = readerDetalles.IsDBNull(3) ? 0 : readerDetalles.GetInt32(3),
                                    Precio_Unitario = readerDetalles.IsDBNull(4) ? 0 : readerDetalles.GetDecimal(4)
                                };

                                venta.Detalles.Add(detalle);
                            }
                        }
                    }
                }
            }

            return ventas;
        }

        public Ventas GetVentaPorId(int idVenta)
        {
            Ventas venta = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Consulta con JOIN para traer la venta y sus detalles
                string query = @"
            SELECT v.Id_Venta, v.Id_Usuario, v.Fecha_Venta, v.Total, v.Nombre,
                   d.Id_Detalle_Venta, d.Id_Producto, d.Nombre_Producto, d.Cantidad, d.Precio_Unitario
            FROM Ventas v
            LEFT JOIN Detalle_Venta d ON v.Id_Venta = d.Id_Venta
            WHERE v.Id_Venta = @Id_Venta";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.Add("@Id_Venta", SqlDbType.Int).Value = idVenta;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (venta == null)
                            {
                                venta = new Ventas
                                {
                                    Id_Venta = reader.GetInt32(0),
                                    Id_Usuario = reader.GetInt32(1),
                                    Fecha_Venta = reader.GetDateTime(2),
                                    Total = reader.GetDecimal(3),
                                    Nombre = reader.IsDBNull(4) ? null : reader.GetString(4),
                                    Detalles = new List<DetalleVenta>()
                                };
                            }

                            if (!reader.IsDBNull(5)) // Si hay detalle
                            {
                                var detalle = new DetalleVenta
                                {
                                    Id_Venta = idVenta,
                                    Id_Detalle_Venta = reader.GetInt32(5),
                                    Id_Producto = reader.GetInt32(6),
                                    Nombre_Producto = reader.IsDBNull(7) ? null : reader.GetString(7),
                                    Cantidad = reader.GetInt32(8),
                                    Precio_Unitario = reader.GetDecimal(9)
                                };
                                venta.Detalles.Add(detalle);
                            }
                        }
                    }
                }
            }

            return venta; // Retorna null si no existe la venta
        }


        public Ventas InsertarVenta(Ventas ventas)
        {
                 // 🧮 1️⃣ Calcular el total automáticamente
               ventas.Total = ventas.Detalles.Sum(d => d.Cantidad * d.Precio_Unitario);

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

               try
               {
                  // 2️⃣ Insertar venta principal
                  using (SqlCommand cmd = new SqlCommand("sp_InsertarVentas", connection, transaction))
                  {
                       cmd.CommandType = CommandType.StoredProcedure;
                       cmd.Parameters.AddWithValue("@Id_Usuario", ventas.Id_Usuario);
                       cmd.Parameters.AddWithValue("@Fecha_Venta", ventas.Fecha_Venta);  
                       cmd.Parameters.AddWithValue("@Total", ventas.Total); // ← ya calculado automáticamente

                      // Parámetro OUTPUT para obtener el Id_Venta generado
                      SqlParameter idVentaParam = new SqlParameter("@Id_Venta", SqlDbType.Int)
                      {
                         Direction = ParameterDirection.Output
                      };
                       cmd.Parameters.Add(idVentaParam);

                       cmd.ExecuteNonQuery();

                        object result = idVentaParam.Value;
                        if (result == DBNull.Value)
                            throw new Exception("No se pudo obtener el Id_Venta generado.");

                        int idVenta = Convert.ToInt32(idVentaParam.Value);

                        // ✅ Asignar el idVenta al objeto principal y a los detalles
                        ventas.Id_Venta = idVenta;
                        foreach (var detalle in ventas.Detalles)
                        {
                            detalle.Id_Venta = ventas.Id_Venta;
                        }

                        // 3️⃣ Insertar cada detalle
                      foreach (DetalleVenta detalle in ventas.Detalles)
                      {
                         using (SqlCommand cmdDetalle = new SqlCommand("_spInsertarDetalleVenta", connection, transaction))
                         {
                             cmdDetalle.CommandType = CommandType.StoredProcedure;
                             cmdDetalle.Parameters.AddWithValue("@Id_Venta", detalle.Id_Venta);
                             cmdDetalle.Parameters.AddWithValue("@Id_Producto", detalle.Id_Producto);
                             cmdDetalle.Parameters.AddWithValue("@Cantidad", detalle.Cantidad);
                             cmdDetalle.Parameters.AddWithValue("@Precio_Unitario", detalle.Precio_Unitario);
                             cmdDetalle.ExecuteNonQuery();
                         }
                      }

                  }

                        // 4️⃣ Confirmar la transacción
                     transaction.Commit();

                    return ventas;
               }
               catch (Exception ex)
               {
                  transaction.Rollback();
                  throw new Exception("Error al registrar la venta: " + ex.Message);
               }

                
            }
        }

        public void EliminaVenta(int idVenta)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    using (SqlCommand cmd = new SqlCommand("sp_EliminarVenta", connection, transaction))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Id_Venta", idVenta);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Error al eliminar la venta: " + ex.Message);
                }
            }
        }


    }
}
