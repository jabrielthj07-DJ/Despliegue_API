using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Capa_Entidades
{
    [Table("Producto")]
    public class Producto
    {
        [Key]
        public int Id_Producto { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres.")]
        public string Nombre_Producto { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(300, ErrorMessage = "La descripción no puede tener más de 300 caracteres.")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El Id_Categoria es Obligatorio")]
        public int Id_Categoria { get; set; }
        
    }
}
