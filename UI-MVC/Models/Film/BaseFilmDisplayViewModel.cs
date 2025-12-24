namespace FilmManagement.UI.Web.Models.Film;

public abstract class BaseFilmDisplayViewModel
{
    public Guid ImdbId { get; set; }
    public string Title { get; set; }
    public string Genre { get; set; } 
    public DateTime ReleaseDate { get; set; }
    public double Rating { get; set; }
}