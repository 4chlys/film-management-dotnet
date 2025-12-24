namespace FilmManagement.UI.Web.Models.Dto.ActorFilm;

public class ActorFilmDto
{
    public Guid ActorId { get; set; }
    public Guid FilmId { get; set; }
    public int? ScreenTime { get; set; }
}