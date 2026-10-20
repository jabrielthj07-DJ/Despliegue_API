using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Capa_Entidades
{
    public class DetalleCompra
    {
        [JsonIgnore]
        public int Id_Compra { get; set; }
        public int Id_Detalle_Compra { get; set; }

        [Required(ErrorMessage = "El Id_Producto es Obligatorio")]
        public int Id_Producto { get; set; }

        [Required(ErrorMessage = "La cantidad es Obligatorio")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "El precio es Obligatorio")]
        public decimal Precio_Unitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
