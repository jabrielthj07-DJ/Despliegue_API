using Capa_Datos;
using Capa_Entidades;
using Microsoft.EntityFrameworkCore;

namespace veterinariweb.Context
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):
            base(options) 
        {
            
        }
        public DbSet<Usuario> Usuarios { get; set; }

        // Tabla Roles 
        public DbSet<Rol> Roles { get; set; }

    }
}
