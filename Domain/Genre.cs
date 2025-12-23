using System.ComponentModel.DataAnnotations;

namespace FilmManagement.BL.Domain;

[Flags]
public enum Genre
{
    None = 0,
    Action = 1 << 0,
    Comedy = 1 << 1,
    Drama = 1 << 2,
    Horror = 1 << 3,
    Romance = 1 << 4,
    [Display(Name = "Science Fiction")]
    SciFi = 1 << 5,
    Thriller = 1 << 6,
    Documentary = 1 << 7,
    Western = 1 << 8,
    Musical = 1 << 9,
    Animation = 1 << 10,
    Fantasy = 1 << 11,
    Short = 1 << 12
}