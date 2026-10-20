using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using System.Text.Json.Serialization;

namespace Capa_Entidades
{

    [Table("Usuario")]
    public class Usuario
    {
        [Key]
        public int Id_Usuario { get; set; }

        [Required(ErrorMessage = "El nombre del usuario es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [StringLength(50, ErrorMessage = "La descripción no puede tener más de 50 caracteres.")]
        public string Correo_Electronico { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(300, ErrorMessage = "La descripción no puede tener más de 300 caracteres.")]
        public string Contraseña { get; set; } = string.Empty;

       // Clave foránea 
    [ForeignKey("Rol")]
    public int Id_Rol { get; set; }

        // Propiedad de navegación
        [JsonIgnore]
        public Rol Rol { get; set; }

        // Propiedad para mostrar el cargo del usuario directamente
        [NotMapped]
        public string cargo { get; set; } = string.Empty;

        // Para actualización
        public class UsuarioUpdateDto
        {
            [Key]

            [Required(ErrorMessage = "El nombre del usuario es obligatorio.")]
            [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres.")]
            public string? Nombre { get; set; }
            [Required(ErrorMessage = "El correo es obligatorio.")]
            [StringLength(50, ErrorMessage = "La descripción no puede tener más de 50 caracteres.")]
            public string? Correo_Electronico { get; set; }
            public string? Contraseña { get; set; } // opcional
            public int? RolId { get; set; } // solo el Id del rol
        }

        // DTO de entrada para crear usuario
        public class UsuarioCreateDto
        {
            [Key]


            [Required(ErrorMessage = "El nombre del usuario es obligatorio.")]
            [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres.")]
            public string Nombre { get; set; } = string.Empty;

            [Required(ErrorMessage = "El correo es obligatorio.")]
            [StringLength(50, ErrorMessage = "La descripción no puede tener más de 50 caracteres.")]
            public string Correo_Electronico { get; set; } = string.Empty;
            public string Contraseña { get; set; } = string.Empty;
            public int Id_Rol { get; set; }
        }

        // DTO de salida para responder al cliente
        public class UsuarioResponseDto
        {
            public int Id_Usuario { get; set; }


            [Required(ErrorMessage = "El nombre del usuario es obligatorio.")]
            [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres.")]
            public string Nombre { get; set; } = string.Empty;
            [Required(ErrorMessage = "El correo es obligatorio.")]
            [StringLength(50, ErrorMessage = "La descripción no puede tener más de 50 caracteres.")]
            public string Correo_Electronico { get; set; } = string.Empty;
            public string Cargo { get; set; } = string.Empty;
        }

    }

}
