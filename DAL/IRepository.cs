using System.Linq.Expressions;
using FilmManagement.BL.Domain;

namespace FilmManagement.DAL;

public interface IRepository
{
    void CreateFilm(Film film);
    Film ReadFilm(Guid imdbId);
    Film ReadFilmWithMaintainer(Guid imdbId);
    Film ReadFilmWithDirector(Guid imdbId);
    Film ReadFilmWithActorsAndDirectors(Guid imdbId);
    IEnumerable<Film> ReadAllFilms();
    IEnumerable<Film> ReadAllFilmsWithDirectors();
    IEnumerable<Film> ReadAllFilmsWithActorsAndDirectors();
    IEnumerable<Film> ReadFilmsByCriteria(Expression<Func<Film, bool>> predicate);
    IEnumerable<Actor> ReadActorsOfFilm(Guid imdbId);
    void UpdateFilm(Film film);
    void DeleteFilm(Film film);
    
    void CreateActorFilm(ActorFilm actorFilm);
    ActorFilm ReadActorFilm(Guid actorId, Guid filmId);
    void UpdateActorFilm(ActorFilm actorFilm);
    void DeleteActorFilm(ActorFilm actorFilm);
    bool ActorFilmExists(Guid actorId, Guid filmId);
    
    void CreateActor(Actor actor);
    Actor ReadActor(Guid imdbId);
    Actor ReadActorWithFilms(Guid imdbId);
    IEnumerable<Actor> ReadAllActors();
    IEnumerable<Actor> ReadAllActorsWithFilms();
    IEnumerable<Actor> ReadActorsByCriteria(Expression<Func<Actor, bool>> predicate);
    void UpdateActor(Actor actor);
    void DeleteActor(Actor actor);
    
    void CreateDirector(Director director); 
    Director ReadDirector(Guid imdbId);
    Director ReadDirectorWithFilms(Guid imdbId);
    Director ReadDirectorByName(string name);
    IEnumerable<Director> ReadAllDirectors();
    IEnumerable<Director> ReadAllDirectorsWithFilms();
    void UpdateDirector(Director director);
    void DeleteDirector(Director director);
}