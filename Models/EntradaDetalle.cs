using System.ComponentModel.DataAnnotations;

namespace GestionEntradaInventario.Models;

public class EntradasDetalle
{
    [Key]
    public int DetalleId { get; set; }

    public int EntradaId { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un producto.")]
    public int ProductoId { get; set; }

    [Range(1, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public double Cantidad { get; set; }

    public double Costo { get; set; }
}
