using System.ComponentModel.DataAnnotations;

namespace MiBackend.DTOs.Requests;

public class UpdateProductoRequest
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
    public string Nombre { get; set; }

    [Required(ErrorMessage = "El precio por libra es requerido")]
    [Range(0.01, 10000, ErrorMessage = "El precio por libra debe estar entre 0.01 y 10000")]
    public decimal PrecioPorLibra { get; set; }

    [Required(ErrorMessage = "El tipo de empaque es requerido")]
    [StringLength(50, ErrorMessage = "El tipo de empaque no puede exceder los 50 caracteres")]
    public string TipoEmpaque { get; set; }
} 