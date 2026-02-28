using System.ComponentModel.DataAnnotations;
using FilmManagement.BL.Domain;
using FilmManagement.BL.Domain.Validation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FilmManagement.UI.Web.Models.Film;

public abstract class BaseFilmFormViewModel
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Title { get; set; }
    [CustomValidation(typeof(EnumValidator), "DefinedValuesOnly")]
    public Genre Genre { get; set; } 
    [DateRange(minYear: 1800, mustBePast: true)]
    public DateTime ReleaseDate { get; set; } = new DateTime(1900, 1, 1);
    [Range(0, 10)]
    public double Rating { get; set; }
    
    public Guid? DirectorId { get; set; }
    public List<Guid> SelectedActorIds { get; set; } = [];
}
