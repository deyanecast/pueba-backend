using System.ComponentModel.DataAnnotations;

namespace MiBackend.DTOs.Requests;

public class UpdateProductoRequest
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
    public required string Nombre { get; set; }

    [Required(ErrorMessage = "La cantidad de libras es requerida")]
    [Range(0.01, 10000, ErrorMessage = "La cantidad debe estar entre 0.01 y 10000 libras")]
    public decimal CantidadLibras { get; set; }

    [Required(ErrorMessage = "El precio por libra es requerido")]
    [Range(0.01, 1000, ErrorMessage = "El precio debe estar entre 0.01 y 1000")]
    public decimal PrecioPorLibra { get; set; }

    [StringLength(50, ErrorMessage = "El tipo de empaque no puede tener más de 50 caracteres")]
    public string? TipoEmpaque { get; set; }
} 