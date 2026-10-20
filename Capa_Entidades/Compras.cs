using System.ComponentModel.DataAnnotations;

namespace Capa_Entidades
{
    public class Compras
    { 
        public int Id_Compra{ get; set;}

        [Required(ErrorMessage = "El Id_Proveedor es Obligatorio")]
        public int Id_Proveedor { get; set; }
        public DateTime Fecha_Compra { get; set; }
        public decimal Total { get; set; }

        [Required(ErrorMessage = "Las Observaciones son obligatorias")]
        public string Observaciones { get; set; }

        //Lista para almacenar los detalles de la venta
        public List<DetalleCompra> Detalles { get; set; }
    }
}
