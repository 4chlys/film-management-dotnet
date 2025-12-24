namespace FilmManagement.UI.Web.Models.Dto.Film;

public class FilmDto
{
    public Guid ImdbId { get; set; }
    public string Title { get; set; }
    public string Genre { get; set; }
    public DateTime ReleaseDate { get; set; }
    public double Rating { get; set; }
    public string DirectorName { get; set; }
}