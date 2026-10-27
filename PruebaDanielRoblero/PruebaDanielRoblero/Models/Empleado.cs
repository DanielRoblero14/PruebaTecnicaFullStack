using System.ComponentModel.DataAnnotations;

namespace PruebaDanielRoblero.Models;

public class Empleado
{
    public int Id { get; set; }

    // Se asignara el codigo al crear el empleado
    [Display(Name = "Código")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(100, ErrorMessage = "Los nombres no pueden superar los 100 caracteres.")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(100, ErrorMessage = "Los apellidos no pueden superar los 100 caracteres.")]
    public string Apellidos { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Fecha de nacimiento")]
    public DateOnly? FechaNacimiento { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un departamento.")]
    [Display(Name = "Departamento")]
    public int DepartamentoId { get; set; }

    // Permite acceder a los datos del departamento relacionado.
    public Departamento? Departamento { get; set; }
}
