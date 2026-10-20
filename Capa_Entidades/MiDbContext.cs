using Microsoft.EntityFrameworkCore;

namespace Capa_Entidades
{
    public class MiDbContext : DbContext
    {
        public MiDbContext(DbContextOptions<MiDbContext> options) : base(options) { }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public  DbSet<Rol> Roles { get; set; }
        public DbSet<Inventario> Inventarios { get; set; }  
        
    }
}

