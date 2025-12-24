using System.ComponentModel.DataAnnotations;
using FilmManagement.BL.Domain.Validation;

namespace FilmManagement.UI.Web.Models.Dto.Actor;

// For PUT requests
public class UpdateActorDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; }
    
    [StringLength(50, MinimumLength = 2)]
    public string Nationality { get; set; }
    
    [Required]
    [DateRange(minYear: 1800, mustBePast: true)]
    public DateTime DateOfBirth { get; set; }
    
    [DateRange(minYear: 1800, mustBePast: true)]
    public DateTime? DateOfDeath { get; set; }
}