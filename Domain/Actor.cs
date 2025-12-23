using System.ComponentModel.DataAnnotations;
using FilmManagement.BL.Domain.Validation;

namespace FilmManagement.BL.Domain;

public class Actor : IValidatableObject
{
    [Key]
    public Guid ImdbId { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(50, MinimumLength = 2)]
    public string Nationality { get; set; } = string.Empty;
    
    [DateRange(minYear: 1800, mustBePast: true)]
    public DateTime DateOfBirth { get; set; }
    
    [DateRange(minYear: 1800, mustBePast: true)]
    public DateTime? DateOfDeath { get; set; }
    
    public int Age => CalculateAge(DateOfBirth, DateOfDeath);

    public ICollection<ActorFilm> ActorFilms { get; init; } = [];
    
    IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
    {
        var errors = new List<ValidationResult>();
        if (DateOfDeath.HasValue && DateOfDeath.Value < DateOfBirth)
        {
            errors.Add(new ValidationResult(
                "Date of death cannot be before date of birth!",
                [nameof(DateOfDeath), nameof(DateOfBirth)]));       
        }
        return errors;
    }

    private static int CalculateAge(DateTime dateOfBirth, DateTime? dateOfDeath = null)
    {
        var referenceDate = dateOfDeath ?? DateTime.Today;
        var age = referenceDate.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > referenceDate.AddYears(-age))
            age--;
        return age;
    }
}