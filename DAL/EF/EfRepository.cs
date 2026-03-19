using System.Linq.Expressions;
using FilmManagement.BL.Domain;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.EF;

namespace FilmManagement.DAL.EF;

public class EfRepository(FilmDbContext context) : IRepository
{
    public void CreateFilm(Film film)
    {
        context.Films.Add(film);
        context.SaveChanges();
    }

    public Film ReadFilm(Guid imdbId)
    {
        return context.Films.Find(imdbId);
    }

    public Film ReadFilmWithMaintainer(Guid imdbId)
    {
        return context.Films.SingleOrDefault(f => f.ImdbId == imdbId);
    }
    
    public Film ReadFilmWithDirector(Guid imdbId)
    {
        return context.Films
            .Include(f => f.Director)
            .SingleOrDefault(f => f.ImdbId == imdbId);
    }

    public Film ReadFilmWithActorsAndDirectors(Guid imdbId)
    {
        return context.Films
            .Include(f => f.Director)
            .Include(f => f.ActorFilms)
            .ThenInclude(af => af.Actor)
            .SingleOrDefault(f => f.ImdbId == imdbId);
    }

    public IEnumerable<Film> ReadAllFilms()
    {
        return context.Films.ToList();
    }

    public IEnumerable<Film> ReadAllFilmsWithDirectors()
    {
        return context.Films
            .Include(f => f.Director)
            .ToList();
    }

    public IEnumerable<Film> ReadAllFilmsWithActorsAndDirectors()
    {
        return context.Films
            .Include(f => f.Director)
            .Include(f => f.ActorFilms)
            .ThenInclude(af => af.Actor)
            .ToList();   
    }

    public IEnumerable<Film> ReadFilmsByCriteria(Expression<Func<Film, bool>> predicate)
    {
        return context.Films
            .Include(f => f.Director)
            .Include(f => f.ActorFilms)
            .ThenInclude(af => af.Actor)
            .Where(predicate)
            .ToList();
    }

    public IEnumerable<Actor> ReadActorsOfFilm(Guid imdbId)
    {
        return context.Actors
            .Where(actor => actor.ActorFilms.Any(af => af.Film.ImdbId == imdbId))
            .ToList();
    }

    public void UpdateFilm(Film film)
    {
        context.Films.Update(film);
        context.SaveChanges();  
    }

    public void DeleteFilm(Film film)
    {
        context.Films.Remove(film);
        context.SaveChanges();       
    }
    
    public void CreateActorFilm(ActorFilm actorFilm)
    {
        if (context.Entry(actorFilm.Actor).State == EntityState.Detached)
            context.Actors.Attach(actorFilm.Actor);

        if (context.Entry(actorFilm.Film).State == EntityState.Detached)
            context.Films.Attach(actorFilm.Film);

        context.ActorFilms.Add(actorFilm);
        context.SaveChanges();
    }
    
    public ActorFilm ReadActorFilm(Guid actorId, Guid filmId)
    {
        return context.ActorFilms.SingleOrDefault(af => af.Actor.ImdbId == actorId && af.Film.ImdbId == filmId);
    }

    public void UpdateActorFilm(ActorFilm actorFilm)
    {
        context.ActorFilms.Update(actorFilm);
        context.SaveChanges();
    }

    public void DeleteActorFilm(ActorFilm actorFilm)
    {
        if (context.Entry(actorFilm.Actor).State == EntityState.Detached)
            context.Actors.Attach(actorFilm.Actor);
    
        if (context.Entry(actorFilm.Film).State == EntityState.Detached)
            context.Films.Attach(actorFilm.Film);

        
        
        context.ActorFilms.Remove(actorFilm);
        context.SaveChanges();
    }

    public bool ActorFilmExists(Guid actorId, Guid filmId)
    {
        return context.ActorFilms.Any(af => 
            Property<Guid>(af, "ActorFK_Shadow") == actorId 
            && Property<Guid>(af, "FilmFK_Shadow") == filmId);
    }

    public void CreateActor(Actor actor)
    {
        context.Actors.Add(actor);
        context.SaveChanges();
    }

    public Actor ReadActor(Guid imdbId)
    {
        return context.Actors.Find(imdbId);
    }

    public Actor ReadActorWithFilms(Guid imdbId)
    {
        return context.Actors
            .Include(a => a.ActorFilms)
            .ThenInclude(af => af.Film)
            .ThenInclude(f => f.Director)
            .SingleOrDefault(a => a.ImdbId == imdbId);
    }

    public IEnumerable<Actor> ReadAllActors()
    {
        return context.Actors.ToList();
    }

    public IEnumerable<Actor> ReadAllActorsWithFilms()
    {
        return context.Actors
            .Include(a => a.ActorFilms)
            .ThenInclude(af => af.Film)
            .ThenInclude(f => f.Director) 
            .ToList();
    }

    public IEnumerable<Actor> ReadActorsByCriteria(Expression<Func<Actor, bool>> predicate)
    {
        return context.Actors
            .Include(a => a.ActorFilms)
            .ThenInclude(af => af.Film)
            .Where(predicate)
            .ToList();
    }

    public void UpdateActor(Actor actor)
    {
        context.Actors.Update(actor);
        context.SaveChanges();
    }

    public void DeleteActor(Actor actor)
    {
        context.Actors.Remove(actor);
        context.SaveChanges();
    }

    public void CreateDirector(Director director)
    {
        context.Directors.Add(director);
        context.SaveChanges();
    }

    public Director ReadDirector(Guid imdbId)
    {
        return context.Directors.Find(imdbId);
    }
    
    public Director ReadDirectorWithFilms(Guid imdbId)
    {
        return context.Directors
            .Include(d => d.Films)
            .SingleOrDefault(d => d.ImdbId == imdbId);
    }
    
    public IEnumerable<Director> ReadAllDirectors()
    {
        return context.Directors.ToList();
    }

    public IEnumerable<Director> ReadAllDirectorsWithFilms()
    {
        return context.Directors
            .Include(d => d.Films)
            .ToList();
    }

    public Director ReadDirectorByName(string name)
    {
        return context.Directors
            .Include(d => d.Films)
            .FirstOrDefault(d => d.Name == name);
    }

    public void UpdateDirector(Director director)
    {
       
        context.Directors.Update(director);
        context.SaveChanges();
    }

    public void DeleteDirector(Director director)
    {
        context.Directors.Remove(director);
        context.SaveChanges();
    }
}