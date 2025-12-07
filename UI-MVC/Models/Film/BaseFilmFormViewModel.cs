using System.ComponentModel.DataAnnotations;
using FilmManagement.BL.Domain;
using FilmManagement.BL.Domain.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FilmManagement.UI.Web.Models.Film;

public abstract class BaseFilmFormViewModel : BaseFilmViewModel
{
    [CustomValidation(typeof(EnumValidator), "DefinedValuesOnly")]
    public new Genre Genre { get; set; }
    
    public Guid? DirectorId { get; set; }
    public List<Guid> SelectedActorIds { get; set; } = [];
    
    public SelectList Directors { get; set; }
    public SelectList Actors { get; set; }
}
