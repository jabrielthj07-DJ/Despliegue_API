using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Capa_Entidades
{
    [Table("Proveedor")]
    public class Proveedor
    {
        [Key]
        public int Id_Proveedor { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "Máximo 50 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50, ErrorMessage = "Máximo 50 caracteres")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo no válido")]
        [StringLength(50)]
        public string Email { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [StringLength(50)]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "La razón social es obligatoria")]
        [StringLength(50)]
        public string Razon_Social { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria")]
        [StringLength(50)]
        public string Direccion { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un producto")]
        [Range(1, int.MaxValue, ErrorMessage = "Id de producto inválido")]
        public int Id_Producto { get; set; }
    }
}
