using System.ComponentModel.DataAnnotations;

namespace Capa_Entidades
{
    public class Ventas
    {
        public int Id_Venta { get; set; }

        [Required(ErrorMessage = "El Id_Usuario es Obligatorio")]
        public int Id_Usuario { get; set; }
        public DateTime Fecha_Venta { get; set; }
        public string Nombre { get; set; }
        public decimal Total { get; set; }

        //Lista para almacenar los detalles de la venta
        public List<DetalleVenta> Detalles { get; set; }

    }
}
