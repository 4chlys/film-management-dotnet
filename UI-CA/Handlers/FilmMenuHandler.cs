using System.ComponentModel.DataAnnotations;
using FilmManagement.BL;
using FilmManagement.BL.Domain;
using FilmManagement.UI.CA.Extensions;
using FilmManagement.UI.CA.Utilities;

namespace FilmManagement.UI.CA.Handlers;

public class FilmMenuHandler(IManager manager)
{
    public void ShowAllFilms()
    {
        Console.WriteLine("\nAll films");
        Console.WriteLine("=========");
        
        var films = manager.GetAllFilmsWithActorsAndDirectors();
        
        if (!films.Any())
        {
            Console.WriteLine("No films found.");
            return;
        }
        
        foreach (var film in films)
        {
            Console.WriteLine(film.GetInfo());
        }
    }

    public void ShowFilmsByGenre()
    {
        var selectedGenres = InputParser.PromptForGenres("Genre(s) - comma-separated");

        if (selectedGenres == 0)
        {
            ValidationHelper.ShowErrorMessage("No valid genres selected.");
            return;
        }

        var filteredFilms = manager.GetFilmsByGenre(selectedGenres);

        Console.WriteLine($"\nFilms with selected genre(s):");
        Console.WriteLine("===========================================");

        if (filteredFilms.Any())
        {
            foreach (var film in filteredFilms)
            {
                Console.WriteLine(film.GetInfo());
            }
        }
        else
        {
            Console.WriteLine("No films found with these genres.");
        }
    }

    public void AddFilm()
    {
        Console.WriteLine("\nAdd film");
        Console.WriteLine("========");

        bool success = false;
        while (!success)
        {
            try
            {
                string title = InputParser.PromptForInput("Title: ");
                Genre genres = InputParser.PromptForGenres("Genre(s) - comma-separated");

                if (genres == 0)
                {
                    Console.WriteLine("Please try again...\n");
                    continue;
                }

                string releaseDateInput = InputParser.PromptForInput("Release date (yyyy-MM-dd): ");
                if (!InputParser.TryParseDateTime(releaseDateInput, out var releaseDate, "release date"))
                {
                    Console.WriteLine("Please try again...\n");
                    continue;
                }

                string ratingInput = InputParser.PromptForInput("Rating (0-10): ");
                if (!InputParser.TryParseDouble(ratingInput, out var rating, "rating"))
                {
                    Console.WriteLine("Please try again...\n");
                    continue;
                }

                var director = GetOrCreateDirector();
                if (director == null)
                {
                    Console.WriteLine("Please try again...\n");
                    continue;
                }

                manager.AddFilm(title, genres, releaseDate, rating, director, null);
                ValidationHelper.ShowSuccessMessage("Film added successfully!");
                success = true;
            }
            catch (ValidationException ex)
            {
                ValidationHelper.ShowValidationErrors(ex.Message);
                Console.WriteLine("Please try again...\n");
            }
        }
    }

    private Director GetOrCreateDirector()
    {
        string directorName = InputParser.PromptForInput("Director name: ");
        var director = manager.GetDirectorByName(directorName);

        if (director != null)
            return director;

        Console.WriteLine("Director not found. Creating new director...");
        
        try
        {
            string country = InputParser.PromptForInput("Country: ");
            string yearStartedInput = InputParser.PromptForInput("Year started (optional): ");
            int? yearStarted = InputParser.ParseOptionalInt(yearStartedInput, "year started");

            string yearEndedInput = InputParser.PromptForInput("Year ended (optional): ");
            int? yearEnded = InputParser.ParseOptionalInt(yearEndedInput, "year ended");

            director = manager.AddDirector(directorName, country, yearStarted, yearEnded);
            ValidationHelper.ShowSuccessMessage("Director created.");
            return director;
        }
        catch (ValidationException ex)
        {
            ValidationHelper.ShowValidationErrors(ex.Message);
            return null;
        }
    }
}