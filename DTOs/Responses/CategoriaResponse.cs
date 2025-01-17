namespace MiBackend.DTOs.Responses
{
    public class CategoriaResponse
    {
        public int CategoriaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool EstaActivo { get; set; }
    }
} 