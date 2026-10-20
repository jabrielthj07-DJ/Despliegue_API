using Capa_Entidades;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using static Capa_Entidades.Usuario;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Capa_Datos
{
     public class UsuarioDato
    {
        private readonly string _connectionString;

        public UsuarioDato(IConfiguration configuration)
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

        // Obtener todos los usuarios
        public IEnumerable<Usuario> GetUsuarios()
        {
            var usuarios = new List<Usuario>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"SELECT 
                u.Id_Usuario,
                u.Nombre,
                u.Correo_Electronico,
                u.Contraseña,
                u.Id_Rol,
                r.Cargo AS Cargo
            FROM Usuario u
            INNER JOIN Roles r ON u.Id_Rol = r.Id_Rol";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var usuario = new Usuario
                        {
                            Id_Usuario = reader.GetInt32(reader.GetOrdinal("Id_Usuario")),
                            Nombre = reader["Nombre"].ToString() ?? string.Empty,
                            Correo_Electronico = reader["Correo_Electronico"].ToString() ?? string.Empty,
                            Contraseña = "********", // Seguridad
                            Id_Rol = reader.GetInt32(reader.GetOrdinal("Id_Rol")),
                            cargo = reader["Cargo"].ToString() ?? string.Empty,
                        };

                        usuarios.Add(usuario);
                    }
                }
            }

            return usuarios;
        }

        // Obtener usuario por Id
        public Usuario? GetUsuariosById(int Id_Usuario)
        {
            Usuario? usuario = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = @" SELECT 
                u.Id_Usuario,
                u.Nombre,
                u.Correo_Electronico,
                u.Contraseña,
                u.Id_Rol,
                r.Cargo AS Cargo
            FROM Usuario u
            INNER JOIN Roles r ON u.Id_Rol = r.Id_Rol
            WHERE u.Id_Usuario = @Id_Usuario";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Id_Usuario", Id_Usuario);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = new Usuario
                            {
                                Id_Usuario = reader.GetInt32(reader.GetOrdinal("Id_Usuario")),
                                Nombre = reader["Nombre"].ToString() ?? string.Empty,
                                Correo_Electronico = reader["Correo_Electronico"].ToString() ?? string.Empty,
                                Contraseña = reader["Contraseña"].ToString() ?? string.Empty,
                                Id_Rol = reader.GetInt32(reader.GetOrdinal("Id_Rol")),
                                cargo = reader["Cargo"].ToString() ?? string.Empty,
                        
                            };
                        }
                    }
                }
            }

            return usuario;
        }

        // Insertar
        public UsuarioResponseDto? Insertar(Usuario usuario)
        {
            int idGenerado = 0;

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                // Ejecutar SP para insertar usuario
                using (SqlCommand cmd = new SqlCommand("sp_InsertarUsuario", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                    cmd.Parameters.AddWithValue("@Correo_Electronico", usuario.Correo_Electronico);
                    cmd.Parameters.AddWithValue("@Contraseña", usuario.Contraseña);
                    cmd.Parameters.AddWithValue("@Id_Rol", usuario.Id_Rol);

                    idGenerado = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Obtener usuario completo con LEFT JOIN para obtener Cargo
                using (SqlCommand cmd2 = new SqlCommand(@"
                SELECT u.Id_Usuario, u.Nombre, u.Correo_Electronico, u.Id_Rol, 
                       ISNULL(r.Cargo,'') AS cargo
                FROM Usuario u
                LEFT JOIN Roles r ON u.Id_Rol = r.Id_Rol
                WHERE u.Id_Usuario = @Id_Usuario", con))
                {
                    cmd2.Parameters.AddWithValue("@Id_Usuario", idGenerado);

                    using (SqlDataReader reader = cmd2.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UsuarioResponseDto
                            {
                                Id_Usuario = reader.GetInt32(reader.GetOrdinal("Id_Usuario")),
                                Nombre = reader["Nombre"].ToString() ?? string.Empty,
                                Correo_Electronico = reader["Correo_Electronico"].ToString() ?? string.Empty,
                                Cargo = reader["cargo"].ToString() ?? string.Empty
                            };
                        }
                    }
                }
            }

            return null;

        }




        // Actualizar
        public Usuario? Actualizar(Usuario usuario)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                // Ejecutar SP para actualizar
                using (SqlCommand cmd = new SqlCommand("sp_ActualizarUsuario", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id_Usuario", usuario.Id_Usuario);
                    cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                    cmd.Parameters.AddWithValue("@Correo_Electronico", usuario.Correo_Electronico);
                    cmd.Parameters.AddWithValue("@Contraseña", usuario.Contraseña);
                    cmd.Parameters.AddWithValue("@Id_Rol", usuario.Id_Rol);

                    cmd.ExecuteNonQuery();
                }

                // Obtener el usuario actualizado con Cargo
                using (SqlCommand cmd2 = new SqlCommand(@"
            SELECT u.Id_Usuario, u.Nombre, u.Correo_Electronico, u.Contraseña, u.Id_Rol, r.Cargo
            FROM Usuario u
            INNER JOIN Roles r ON u.Id_Rol = r.Id_Rol
            WHERE u.Id_Usuario = @Id_Usuario", con))
                {
                    cmd2.Parameters.AddWithValue("@Id_Usuario", usuario.Id_Usuario);

                    using (SqlDataReader reader = cmd2.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Usuario
                            {
                                Id_Usuario = reader.GetInt32(reader.GetOrdinal("Id_Usuario")),
                                Nombre = reader["Nombre"].ToString() ?? string.Empty,
                                Correo_Electronico = reader["Correo_Electronico"].ToString() ?? string.Empty,
                                Contraseña = reader["Contraseña"].ToString() ?? string.Empty,
                                Id_Rol = reader.GetInt32(reader.GetOrdinal("Id_Rol")),
                                cargo = reader["cargo"].ToString() ?? string.Empty
                            };
                        }
                    }
                }
            }

            return null;
        }

        // Eliminar
        public bool Eliminar(int idUsuario)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand("sp_EliminarUsuario", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Usuario", idUsuario);

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    return filasAfectadas > 0; // true si se eliminó al menos un registro
                }
            }
        }

        
    }
}
