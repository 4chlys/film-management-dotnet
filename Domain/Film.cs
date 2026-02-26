using System.ComponentModel.DataAnnotations;
using FilmManagement.BL.Domain.Validation;
using Microsoft.AspNetCore.Identity;

namespace FilmManagement.BL.Domain;

public class Film
{
    [Key]
    public Guid ImdbId { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;
    
    [CustomValidation(typeof(EnumValidator), "DefinedValuesOnly")]
    public Genre Genre { get; set; }
    
    [DateRange(minYear: 1800, mustBePast: true)]
    public DateTime ReleaseDate { get; set; }
    
    
    public double Rating { get; set; }
    
    public Director Director { get; set; }

    public ICollection<ActorFilm> ActorFilms { get; init; } = [];
    
    public IdentityUser Maintainer { get; set; }
}