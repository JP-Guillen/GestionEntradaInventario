namespace GestionEntradaInventario.Models;
using System.ComponentModel.DataAnnotations;

public class Productos
{
    [Key]
    public int ProductoId { get; set; }

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    public string? Descripcion { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El costo debe ser mayor a 0.")]
    public double Costo { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
    public double Precio { get; set; }

    public double Existencia { get; set; }
}