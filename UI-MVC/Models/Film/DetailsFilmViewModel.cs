using System.ComponentModel.DataAnnotations;

namespace FilmManagement.UI.Web.Models.Film;

public class DetailsFilmViewModel : BaseFilmViewModel
{
    [Display(Name = "Director")]
    public DirectorInfo Director { get; set; }
    
    [Display(Name = "Actors")]
    public List<ActorInfo> Actors { get; set; } = new();
    
    public class DirectorInfo
    {
        public Guid ImdbId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    
    public class ActorInfo
    {
        public Guid ImdbId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public int? ScreenTime { get; set; }
    }
}