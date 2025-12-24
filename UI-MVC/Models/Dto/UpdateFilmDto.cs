using System.ComponentModel.DataAnnotations;
using FilmManagement.BL.Domain;
using FilmManagement.BL.Domain.Validation;

namespace FilmManagement.UI.Web.Models.Dto;

public class UpdateFilmDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;
    
    public Genre Genre { get; set; }
    
    [DateRange(minYear: 1800, mustBePast: true)]
    [Display(Name = "Release Date")]
    public DateTime ReleaseDate { get; set; }
    
    [Range(0, 10)]
    public double Rating { get; set; }
    
    public Guid? DirectorId { get; set; }
}