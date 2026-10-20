using Capa_Entidades;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Capa_Datos
{
    public class CompraDato
    {
        //declaracion de la cadena de conecxion con readonly
        private readonly string _connectionString;

        public CompraDato(IConfiguration configuration)
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

        // Obtener compras

        public IEnumerable<Compras> GetComprasConDetalles()
        {
            var compras = new List<Compras>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                //  Obtener compras
                string queryCompras = @"
        SELECT Id_Compra, Id_Proveedor, Fecha_Compra, Total, Observaciones
        FROM Compras";
                                                                                     
                using (SqlCommand cmdCompras = new SqlCommand(queryCompras, connection))
                using (SqlDataReader readerCompras = cmdCompras.ExecuteReader())
                {
                    while (readerCompras.Read())
                    {
                        var compra = new Compras
                        {
                            Id_Compra = readerCompras.GetInt32(0),                                                                                 
                            Id_Proveedor = readerCompras.GetInt32(1),
                            Fecha_Compra = readerCompras.GetDateTime(2),
                            Total = readerCompras.GetDecimal(3),
                            Observaciones = readerCompras.IsDBNull(4)
                                ? null
                                : readerCompras.GetString(4),

                            Detalles = new List<DetalleCompra>()
                        };

                        compras.Add(compra);
                    }
                }

                //  Obtener detalles de cada compra
                foreach (var compra in compras)
                {
                    string queryDetalles = @"
            SELECT Id_Detalle_Compra,
                   Id_Producto,
                   Cantidad,
                   Precio_Unitario,
                   Subtotal
            FROM Detalle_Compras
            WHERE Id_Compra = @Id_Compra";

                    using (SqlCommand cmdDetalles = new SqlCommand(queryDetalles, connection))
                    {
                        cmdDetalles.Parameters.AddWithValue("@Id_Compra", compra.Id_Compra);

                        using (SqlDataReader readerDetalles = cmdDetalles.ExecuteReader())
                        {
                            while (readerDetalles.Read())
                            {
                                var detalle = new DetalleCompra
                                {
                                    Id_Compra = compra.Id_Compra,
                                    Id_Detalle_Compra = readerDetalles.GetInt32(0),
                                    Id_Producto = readerDetalles.GetInt32(1),
                                    Cantidad = readerDetalles.GetInt32(2),
                                    Precio_Unitario = readerDetalles.GetDecimal(3),
                                    Subtotal = readerDetalles.GetDecimal(4)
                                };

                                compra.Detalles.Add(detalle);
                            }
                        }
                    }
                }
            }

            return compras;
        }

        //Obtener compras por id

        public Compras GetCompraPorId(int idCompra)
        {
            Compras compra = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"
        SELECT c.Id_Compra,
               c.Id_Proveedor,
               c.Fecha_Compra,
               c.Total,
               c.Observaciones,

               d.Id_Detalle_Compra,
               d.Id_Producto,
               d.Cantidad,
               d.Precio_Unitario,
               d.Subtotal

        FROM Compras c
        LEFT JOIN Detalle_Compras d
            ON c.Id_Compra = d.Id_Compra

        WHERE c.Id_Compra = @Id_Compra";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.Add("@Id_Compra", SqlDbType.Int).Value = idCompra;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (compra == null)
                            {
                                compra = new Compras
                                {
                                    Id_Compra = reader.GetInt32(0),
                                    Id_Proveedor = reader.GetInt32(1),
                                    Fecha_Compra = reader.GetDateTime(2),
                                    Total = reader.GetDecimal(3),
                                    Observaciones = reader.IsDBNull(4)
                                        ? null
                                        : reader.GetString(4),

                                    Detalles = new List<DetalleCompra>()
                                };
                            }

                            if (!reader.IsDBNull(5))
                            {
                                var detalle = new DetalleCompra
                                {
                                    Id_Compra = idCompra,
                                    Id_Detalle_Compra = reader.GetInt32(5),
                                    Id_Producto = reader.GetInt32(6),
                                    Cantidad = reader.GetInt32(7),
                                    Precio_Unitario = reader.GetDecimal(8),
                                    Subtotal = reader.GetDecimal(9)
                                };

                                compra.Detalles.Add(detalle);
                            }
                        }
                    }
                }
            }

            return compra;
        }

        // insertar compras

        public Compras InsertarCompra(Compras compras)
        {
            //  Calcular total
            compras.Total = compras.Detalles.Sum(d => d.Cantidad * d.Precio_Unitario);

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // Insertar compra principal
                    using (SqlCommand cmd = new SqlCommand("sp_InsertarCompras", connection, transaction))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Id_Proveedor", compras.Id_Proveedor);
                        cmd.Parameters.AddWithValue("@Fecha_Compra", compras.Fecha_Compra);
                        cmd.Parameters.AddWithValue("@Total", compras.Total);
                        cmd.Parameters.AddWithValue("@Observaciones", compras.Observaciones);

                        SqlParameter idCompraParam = new SqlParameter("@Id_Compra", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };

                        cmd.Parameters.Add(idCompraParam);

                        cmd.ExecuteNonQuery();

                        int idCompra = Convert.ToInt32(idCompraParam.Value);

                        compras.Id_Compra = idCompra;

                        foreach (var detalle in compras.Detalles)
                        {
                            detalle.Id_Compra = idCompra;
                        }

                        // 3️⃣ Insertar detalles
                        foreach (DetalleCompra detalle in compras.Detalles)
                        {
                            using (SqlCommand cmdDetalle = new SqlCommand("_spInsertarDetalleCompra", connection, transaction))
                            {
                                cmdDetalle.CommandType = CommandType.StoredProcedure;

                                cmdDetalle.Parameters.AddWithValue("@Id_Compra", detalle.Id_Compra);
                                cmdDetalle.Parameters.AddWithValue("@Id_Producto", detalle.Id_Producto);
                                cmdDetalle.Parameters.AddWithValue("@Cantidad", detalle.Cantidad);
                                cmdDetalle.Parameters.AddWithValue("@Precio_Unitario", detalle.Precio_Unitario);

                                cmdDetalle.ExecuteNonQuery();
                            }
                        }
                    }

                    transaction.Commit();

                    return compras;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Error al registrar la compra: " + ex.Message);
                }
            }

        }

        //eliminar compras

        public void EliminarCompra(int idCompra)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    using (SqlCommand cmd = new SqlCommand("sp_EliminarCompra", connection, transaction))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Id_Compra", idCompra);

                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Error al eliminar la compra: " + ex.Message);
                }
            }
        }
    }
}
