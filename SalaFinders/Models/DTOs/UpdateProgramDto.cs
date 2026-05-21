using System.ComponentModel.DataAnnotations;

namespace SalaFinders.Models.DTOs;

public class UpdateProgramDto
{
    [Required(ErrorMessage = "La carrera es obligatoria")]
    public string Program { get; set; } = string.Empty;
}
