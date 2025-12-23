using System.ComponentModel.DataAnnotations;

namespace FilmManagement.UI.Web.Models.Film;

public class DetailsFilmViewModel : BaseFilmDisplayViewModel
{
    public DirectorInfo Director { get; set; }
    public List<ActorInfo> Actors { get; set; } = [];
    
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