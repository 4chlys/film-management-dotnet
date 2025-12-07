using System.ComponentModel.DataAnnotations;

namespace FilmManagement.UI.Web.Models.Film;

public class DeleteFilmViewModel : BaseFilmViewModel
{
    [Display(Name = "Director")]
    public string DirectorName { get; set; } = string.Empty;
    
    [Display(Name = "Release Year")]
    public int ReleaseYear => ReleaseDate.Year;
}