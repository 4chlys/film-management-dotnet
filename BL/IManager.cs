using System.ComponentModel.DataAnnotations;
using FilmManagement.BL.Domain;
using FilmManagement.DAL;

namespace FilmManagement.BL;

public interface IManager
{
    Film AddFilm(string title, Genre genre, DateTime releaseDate, double rating, Director director);
    void AddActorToFilm(Guid filmId, Guid actorId, int? screenTime);
    Film GetFilm(Guid imdbId); 
    Film GetFilmWithActorsAndDirectors(Guid imdbId);
    IEnumerable<Film> GetAllFilms();
    IEnumerable<Film> GetAllFilmsWithDirectors();
    IEnumerable<Film> GetAllFilmsWithActorsAndDirectors();
    IEnumerable<Film> GetFilmsByGenre(Genre genre); 
    void ChangeFilm(Film film);
    void RemoveFilm(Film film);
    void RemoveActorFromFilm(Guid filmId, Guid actorId);
    
    Actor AddActor(string name, string nationality, DateTime dateOfBirth, DateTime? dateOfDeath);
    Actor GetActor(Guid imdbId); 
    Actor GetActorWithFilms(Guid imdbId);
    IEnumerable<Actor> GetAllActors();
    IEnumerable<Actor> GetAllActorsWithFilms();
    IEnumerable<Actor> GetActorsByCriteria(string nameFilter, int? minimumAge);
    IEnumerable<Actor> GetActorsOfFilm(Guid imdbId);
    void ChangeActor(Actor actor);
    void RemoveActor(Actor actor);
    
    Director AddDirector(string name, string country, int? yearStarted, int? yearEnded); 
    Director GetDirector(Guid imdbId);
    Director GetDirectorWithFilms(Guid imdbId);
    Director GetDirectorByName(string name);
    IEnumerable<Director> GetAllDirectors();
    IEnumerable<Director> GetAllDirectorsWithFilms();
    void ChangeDirector(Director director);
    void RemoveDirector(Director director);
    
    IEnumerable<Genre> GetAllGenres();
}