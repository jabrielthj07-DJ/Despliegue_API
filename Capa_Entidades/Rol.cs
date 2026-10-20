using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Capa_Entidades
{
    [Table("Roles")]
    public class Rol
    {

        [Key] // ✅ Define la clave primaria
        public int Id_Rol { get; set; }

        public string cargo { get; set; }

        // Relación opcional con usuarios
        [JsonIgnore]
        public ICollection<Usuario> Usuarios { get; set; }
    }
}
