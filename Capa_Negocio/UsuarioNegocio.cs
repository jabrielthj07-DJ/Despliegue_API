using Capa_Datos;
using Capa_Entidades;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using static Capa_Entidades.Usuario;

namespace Capa_Negocio
{
    public class UsuarioNegocio
    {
        private readonly UsuarioDato _usuarioDato;
        private readonly string _ConnectionString;

        public UsuarioNegocio(IConfiguration configuration)
        {
            string connectionString = (configuration.GetConnectionString("Connection") ?? configuration.GetConnectionString("DefaultConnection"));
            _usuarioDato = new UsuarioDato(configuration); ; // inicialización

        }

        //obtener todos los usuarios
        public IEnumerable<Usuario> GetUsuarios()
        {
            return _usuarioDato.GetUsuarios();
        }
        //obtener usuarios por id
        public Usuario? GetUsuarioById(int id)
        {
            return _usuarioDato.GetUsuariosById(id);
        }

        //crud

        public UsuarioResponseDto? Insertar(Usuario user, string clavePlano)
        {
            // Hashear contraseña
            byte[] salt = GenerarSalt();
            user.Contraseña = HashearClave(clavePlano, salt);

            // Insertar en DB y retornar DTO
            UsuarioResponseDto? usuarioInsertado = _usuarioDato.Insertar(user);

            return usuarioInsertado;
        }

        //Metodo de Actualizar Usuario
        public void UpdateUsuario(Usuario user, string? clavePlano = null)
        {
            if (!string.IsNullOrEmpty(clavePlano))
            {
                byte[] salt = GenerarSalt();
                user.Contraseña = HashearClave(clavePlano, salt);
            }

            _usuarioDato.Actualizar(user);
        }

        //Metodo de Eliminar Usuario
        public void DeleteUsuario(int id)
        {
            _usuarioDato.Eliminar(id);
        }

        //  Métodos de seguridad 
        public byte[] GenerarSalt(int tamaño = 16)
        {
            byte[] salt = new byte[tamaño];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }

        public string HashearClave(string clave, byte[] salt)
        {
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(clave))
            {
                Salt = salt,
                DegreeOfParallelism = 2,
                Iterations = 4,
                MemorySize = 1024
            };

            var hashBytes = argon2.GetBytes(16);
            return Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hashBytes);
        }


        
    }
}
