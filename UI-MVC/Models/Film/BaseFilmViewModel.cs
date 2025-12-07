using System.ComponentModel.DataAnnotations;
using FilmManagement.BL.Domain;
using FilmManagement.BL.Domain.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FilmManagement.UI.Web.Models.Film;

public abstract class BaseFilmViewModel
{
    public Guid ImdbId { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 3)]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;
    
    [Display(Name = "Genre")]
    public string Genre { get; set; } = string.Empty;
    
    [DateRange(minYear: 1800, mustBePast: true)]
    [Display(Name = "Release Date")]
    public DateTime ReleaseDate { get; set; }
    
    [Range(0, 10)]
    [Display(Name = "Rating")]
    public double Rating { get; set; }
}