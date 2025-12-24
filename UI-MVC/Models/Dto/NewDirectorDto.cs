using System.ComponentModel.DataAnnotations;
using FilmManagement.BL.Domain.Validation;

namespace FilmManagement.UI.Web.Models.Dto;

public class NewDirectorDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(50, MinimumLength = 2)]
    public string Country { get; set; } = string.Empty;
    
    [DateRange(minYear: 1800, mustBePast: true)]
    public int? CareerStart { get; set; } 
    
    [DateRange(minYear: 1800, mustBePast: true)]
    public int? CareerEnd { get; set; }
}