namespace FilmManagement.UI.Web.Models.Dto;

public class DirectorDto
{
    public Guid ImdbId { get; set; }
    public string Name { get; set; }
    public string Country { get; set; }
    public int? CareerStart { get; set; }
    public int? CareerEnd { get; set; }
}