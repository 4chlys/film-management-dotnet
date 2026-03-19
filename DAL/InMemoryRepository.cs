using System.Linq.Expressions;
using FilmManagement.BL.Domain;
using FilmManagement.DAL.EF;

namespace FilmManagement.DAL;

public class InMemoryRepository : IRepository
{
    //Film operations   
    public void CreateFilm(Film film)
    {
        film.ImdbId = Guid.NewGuid();
        InMemoryStorage.Films.Add(film);
    }

    public void CreateActorFilm(ActorFilm actorFilm)
    {
        throw new NotImplementedException();
    }

    public Film ReadFilm(Guid imdbId)
    {
        return InMemoryStorage.Films.SingleOrDefault(f => f.ImdbId == imdbId);       
    }

    public Film ReadFilmWithMaintainer(Guid imdbId)
    {
        throw new NotImplementedException();
    }

    public Film ReadFilmWithDirector(Guid imdbId)
    {
        throw new NotImplementedException();
    }

    public Film ReadFilmWithActorsAndDirectors(Guid imdbId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Film> ReadAllFilms()
    {
        return InMemoryStorage.Films;
    }

    public IEnumerable<Film> ReadAllFilmsWithDirectors()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Film> ReadAllFilmsWithActorsAndDirectors()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Film> ReadFilmsByCriteria(Expression<Func<Film, bool>> predicate)
    {
        return InMemoryStorage.Films.AsQueryable().Where(predicate).ToList();
    }

    public IEnumerable<Actor> ReadActorsOfFilm(Guid imdbId)
    {
        throw new NotImplementedException();
    }

    public void UpdateFilm(Film film)
    {
        var existing = InMemoryStorage.Films.SingleOrDefault(f => f.ImdbId == film.ImdbId);
        if (existing == null)
        {
            CreateFilm(film);
            return;
        }
        existing.Title = film.Title;
        existing.Genre = film.Genre;
        existing.ReleaseDate = film.ReleaseDate;
        existing.Rating = film.Rating;
        existing.Director = film.Director;
    }
    
    public void DeleteFilm(Film film)
    {
        InMemoryStorage.Films.Remove(film);
    }
    
    public ActorFilm ReadActorFilm(Guid actorId, Guid filmId)
    {
        throw new NotImplementedException();
    }

    public void UpdateActorFilm(ActorFilm actorFilm)
    {
        throw new NotImplementedException();
    }

    public void DeleteActorFilm(ActorFilm ActorId)
    {
        throw new NotImplementedException();
    }

    public bool ActorFilmExists(Guid actorId, Guid filmId)
    {
        throw new NotImplementedException();
    }

    //Actor operations  
    public void CreateActor(Actor actor)
    {
        actor.ImdbId = Guid.NewGuid();
        InMemoryStorage.Actors.Add(actor);
    }

    public Actor ReadActor(Guid imdbId)
    {
        return InMemoryStorage.Actors.SingleOrDefault(a => a.ImdbId == imdbId);       
    }

    public Actor ReadActorWithFilms(Guid imdbId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Actor> ReadAllActors()
    {
        return InMemoryStorage.Actors;
    }

    public IEnumerable<Actor> ReadAllActorsWithFilms()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Actor> ReadActorsByCriteria(Expression<Func<Actor, bool>> predicate)
    {
        return InMemoryStorage.Actors.AsQueryable().Where(predicate).ToList();
    }

    public void UpdateActor(Actor actor)
    {
        var existing = InMemoryStorage.Actors.SingleOrDefault(a => a.ImdbId == actor.ImdbId);
        if (existing == null)
        {
            CreateActor(actor);
            return;
        }
        existing.Name = actor.Name;
        existing.Nationality = actor.Nationality;
        existing.DateOfBirth = actor.DateOfBirth;
    }

    public void DeleteActor(Actor actor)
    {
        InMemoryStorage.Actors.Remove(actor);
    }
    
    //Director operations
    public void CreateDirector(Director director)
    {
        director.ImdbId = Guid.NewGuid();
        InMemoryStorage.FilmDirectors.Add(director);
    }

    public Director ReadDirector(Guid imdbId)
    {
        return InMemoryStorage.FilmDirectors.SingleOrDefault(d => d.ImdbId == imdbId);       
    }

    public Director ReadDirectorWithFilms(Guid imdbId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Director> ReadAllDirectors()
    {
        return InMemoryStorage.FilmDirectors;
    }

    public IEnumerable<Director> ReadAllDirectorsWithFilms()
    {
        throw new NotImplementedException();
    }

    public Director ReadDirectorByName(string name)
    {
        return InMemoryStorage.FilmDirectors.FirstOrDefault(d => d.Name == name);
    }

    public void UpdateDirector(Director director)
    {
        var existing = InMemoryStorage.FilmDirectors.FirstOrDefault(d => d.ImdbId == director.ImdbId);
        if (existing == null)
        {
            CreateDirector(director);
            return;
        }
        existing.Name = director.Name;
        existing.Country = director.Country;
        existing.CareerStart = director.CareerStart;
        
    }
    
    public void DeleteDirector(Director director)
    {
        InMemoryStorage.FilmDirectors.Remove(director);       
    }
}