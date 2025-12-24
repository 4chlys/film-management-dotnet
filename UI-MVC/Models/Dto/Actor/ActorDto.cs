namespace FilmManagement.UI.Web.Models.Dto.Actor;

// For GET responses
public class ActorDto
{
    public Guid ImdbId { get; set; }
    public string Name { get; set; }
    public string Nationality { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime? DateOfDeath { get; set; }
    public int Age { get; set; }
}