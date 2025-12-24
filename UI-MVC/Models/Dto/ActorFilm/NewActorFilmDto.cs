using System.ComponentModel.DataAnnotations;

namespace FilmManagement.UI.Web.Models.Dto.ActorFilm;

public class NewActorFilmDto
{
    [Required]
    public Guid ActorId { get; set; }
    
    [Required]
    public Guid FilmId { get; set; }
    
    [Range(0, int.MaxValue)]
    public int? ScreenTime { get; set; }
}