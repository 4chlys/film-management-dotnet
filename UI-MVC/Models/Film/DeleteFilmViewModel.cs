namespace FilmManagement.UI.Web.Models.Film;

public class DeleteFilmViewModel : BaseFilmDisplayViewModel
{
    public string DirectorName { get; set; }
    public int ReleaseYear => ReleaseDate.Year;
}