using System.ComponentModel.DataAnnotations;

namespace MiBackend.DTOs.Requests;

public class UpdateStatusRequest
{
    [Required(ErrorMessage = "El estado es requerido")]
    public bool IsActive { get; set; }
} 