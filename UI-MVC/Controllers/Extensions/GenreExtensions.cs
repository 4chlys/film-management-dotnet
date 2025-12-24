namespace FilmManagement.UI.Web.Controllers.Extensions;

public static class GenreExtensions
{
    public static string ToDisplayString(this BL.Domain.Genre genre)
    {
        return string.Join(", ",
            Enum.GetValues<BL.Domain.Genre>()
                .Where(g => g != BL.Domain.Genre.None && genre.HasFlag(g))
                .Select(g => g.ToString())
        );
    }
}