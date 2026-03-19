using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using FilmManagement.BL.Domain;
using FilmManagement.BL.Domain.Validation;
using FilmManagement.DAL;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FilmManagement.BL;

public class Manager(IRepository repository) : IManager
{
    public Film AddFilm(string title, Genre genre, DateTime releaseDate, double rating, Director director, IdentityUser maintainer)
    {
        var filmToCreate = new Film
        {
            Title = title,
            Genre = genre,
            ReleaseDate = releaseDate,
            Rating = rating,
            Director = director,
            Maintainer = maintainer
        };

        EntityValidator.ValidateEntity(filmToCreate);
        try
        {
            repository.CreateFilm(filmToCreate);
        }
        catch (DbUpdateException)
        {
            throw new ValidationException("A film with this title already exists.");
        }


        return filmToCreate;
    }

    public Film GetFilm(Guid imdbId)
    {
        return repository.ReadFilm(imdbId);
    }

    public Film GetFilmWithMaintainer(Guid imdbId)
    {
        return repository.ReadFilmWithMaintainer(imdbId);
    }
    
    public Film GetFilmWithDirector(Guid imdbId)
    {
        return repository.ReadFilmWithDirector(imdbId);
    }

    public Film GetFilmWithActorsAndDirectors(Guid imdbId)
    {
        return repository.ReadFilmWithActorsAndDirectors(imdbId);
    }

    public IEnumerable<Film> GetAllFilms()
    {
        return repository.ReadAllFilms();
    }

    public IEnumerable<Film> GetAllFilmsWithDirectors()
    {
        return repository.ReadAllFilmsWithDirectors();
    }

    public IEnumerable<Film> GetAllFilmsWithActorsAndDirectors()
    {
        return repository.ReadAllFilmsWithActorsAndDirectors();
    }

    public IEnumerable<Film> GetFilmsByGenre(Genre genre)
    {
        Expression<Func<Film, bool>> predicate = f => (f.Genre & genre) != 0;

        return repository.ReadFilmsByCriteria(predicate);
    }

    public void ChangeFilm(Film film)
    {
        EntityValidator.ValidateEntity(film);
        repository.UpdateFilm(film);
    }

    public void RemoveFilm(Film film)
    {
        repository.DeleteFilm(film);
    }

    public Film AddFilmWithActors(string title, Genre genre, DateTime releaseDate, double rating,
        Guid? directorId, List<Guid> actorIds, IdentityUser maintainer)
    {
        Director director = null;
        if (directorId.HasValue)
        {
            director = repository.ReadDirector(directorId.Value);
            if (director == null)
                throw new InvalidOperationException($"Director with ID {directorId.Value} not found.");
        }

        var actorsToAdd = new List<Actor>();
        if (actorIds != null && actorIds.Count != 0)
        {
            foreach (var actorId in actorIds)
            {
                var actor = repository.ReadActor(actorId);
                if (actor == null)
                    throw new InvalidOperationException($"Actor with ID {actorId} not found.");
                actorsToAdd.Add(actor);
            }
        }

        var filmToCreate = AddFilm(title, genre, releaseDate, rating, director, maintainer);

        foreach (var actor in actorsToAdd)
        {
            var actorFilm = new ActorFilm
            {
                Actor = actor,
                Film = filmToCreate,
                ScreenTime = null
            };

            EntityValidator.ValidateEntity(actorFilm);
            repository.CreateActorFilm(actorFilm);
        }

        return filmToCreate;
    }

    public void ChangeFilmWithActors(Film film, Guid? directorId, List<Guid> actorIds)
    {
        if (film == null)
            throw new ArgumentNullException(nameof(film));

        if (directorId.HasValue)
        {
            var director = repository.ReadDirector(directorId.Value);
            if (director == null)
                throw new InvalidOperationException($"Director with ID {directorId.Value} not found.");
            film.Director = director;
        }
        else
        {
            film.Director = null;
        }

        EntityValidator.ValidateEntity(film);

        repository.UpdateFilm(film);

        SyncFilmActors(film.ImdbId, actorIds ?? new List<Guid>());
    }

    private void SyncFilmActors(Guid filmId, List<Guid> newActorIds)
    {
        var currentActors = repository.ReadActorsOfFilm(filmId).ToList();
        var currentActorIds = currentActors.Select(a => a.ImdbId).ToHashSet();
        var newActorIdSet = newActorIds.ToHashSet();

        foreach (var actorId in newActorIds)
        {
            if (!currentActorIds.Contains(actorId))
            {
                var actor = repository.ReadActor(actorId);
                if (actor == null)
                    throw new InvalidOperationException($"Actor with ID {actorId} not found.");
            }
        }

        var actorsToRemove = currentActorIds.Except(newActorIdSet);
        foreach (var actorId in actorsToRemove)
        {
            RemoveActorFromFilm(filmId, actorId);
        }

        var actorsToAdd = newActorIdSet.Except(currentActorIds);
        foreach (var actorId in actorsToAdd)
        {
            AddActorToFilm(filmId, actorId, null);
        }
    }

    public void AddActorToFilm(Guid filmId, Guid actorId, int? screenTime)
    {
        var film = repository.ReadFilm(filmId);
        var actor = repository.ReadActor(actorId);

        if (film == null)
            throw new InvalidOperationException("Film not found.");
        if (actor == null)
            throw new InvalidOperationException("Actor not found.");

        var actorFilm = new ActorFilm
        {
            Actor = actor,
            Film = film,
            ScreenTime = screenTime
        };

        EntityValidator.ValidateEntity(actorFilm);

        if (repository.ActorFilmExists(actorId, filmId))
            throw new InvalidOperationException(
                "This actor is already associated with this film.");

        repository.CreateActorFilm(actorFilm);
    }

    public void RemoveActorFromFilm(Guid filmId, Guid actorId)
    {
        if (!repository.ActorFilmExists(actorId, filmId))
            throw new InvalidOperationException(
                "This actor is not associated with this film.");

        var actorFilm = repository.ReadActorFilm(actorId, filmId);

        repository.DeleteActorFilm(actorFilm);
    }

    public void UpdateActorFilmScreenTime(Guid filmId, Guid actorId, int? screenTime)
    {
        var actorFilm = repository.ReadActorFilm(actorId, filmId);
        if (actorFilm == null)
            throw new InvalidOperationException("Actor-Film relationship not found.");

        actorFilm.ScreenTime = screenTime;
        EntityValidator.ValidateEntity(actorFilm);
        repository.UpdateActorFilm(actorFilm);
    }


    public Actor AddActor(string name, string nationality, DateTime dateOfBirth, DateTime? dateOfDeath)
    {
        var actorToCreate = new Actor
        {
            Name = name,
            Nationality = nationality,
            DateOfBirth = dateOfBirth,
            DateOfDeath = dateOfDeath
        };

        EntityValidator.ValidateEntity(actorToCreate);
        repository.CreateActor(actorToCreate);
        return actorToCreate;
    }

    public Actor GetActor(Guid imdbId)
    {
        return repository.ReadActor(imdbId);
    }

    public Actor GetActorWithFilms(Guid imdbId)
    {
        return repository.ReadActorWithFilms(imdbId);
    }

    public IEnumerable<Actor> GetAllActors()
    {
        return repository.ReadAllActors();
    }

    public IEnumerable<Actor> GetAllActorsWithFilms()
    {
        return repository.ReadAllActorsWithFilms();
    }

    public IEnumerable<Actor> GetActorsByCriteria(string nameFilter, int? minimumAge)
    {
        Expression<Func<Actor, bool>> predicate = actor => true;

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            var upperFilter = nameFilter.ToUpper();
            predicate = CombinePredicates(predicate,
                a => a.Name.ToUpper().Contains(upperFilter));
        }

        if (minimumAge.HasValue)
        {
            var maxDateOfBirth = DateTime.Today.AddYears(-minimumAge.Value);
            predicate = CombinePredicates(predicate,
                a => a.DateOfBirth <= maxDateOfBirth);
        }

        return repository.ReadActorsByCriteria(predicate);
    }

    public IEnumerable<Actor> GetActorsOfFilm(Guid imdbId)
    {
        return repository.ReadActorsOfFilm(imdbId);
    }

    public void ChangeActor(Actor actor)
    {
        EntityValidator.ValidateEntity(actor);
        repository.UpdateActor(actor);
    }

    public void RemoveActor(Actor actor)
    {
        repository.DeleteActor(actor);
    }

    public Director AddDirector(string name, string country, int? yearStarted, int? yearEnded)
    {
        var filmDirectorToCreate = new Director
        {
            Name = name,
            Country = country,
            CareerStart = yearStarted,
            CareerEnd = yearEnded
        };

        EntityValidator.ValidateEntity(filmDirectorToCreate);
        repository.CreateDirector(filmDirectorToCreate);
        return filmDirectorToCreate;
    }

    public Director GetDirector(Guid imdbId)
    {
        return repository.ReadDirector(imdbId);
    }

    public Director GetDirectorWithFilms(Guid imdbId)
    {
        return repository.ReadDirectorWithFilms(imdbId);
    }

    public Director GetDirectorByName(string name)
    {
        return repository.ReadDirectorByName(name);
    }

    public IEnumerable<Director> GetAllDirectors()
    {
        return repository.ReadAllDirectors();
    }

    public IEnumerable<Director> GetAllDirectorsWithFilms()
    {
        return repository.ReadAllDirectorsWithFilms();
    }

    public void ChangeDirector(Director director)
    {
        EntityValidator.ValidateEntity(director);
        repository.UpdateDirector(director);
    }

    public void RemoveDirector(Director director)
    {
        repository.DeleteDirector(director);
    }

    public IEnumerable<Genre> GetAllGenres()
    {
        return Enum.GetValues<Genre>();
    }

    private Expression<Func<T, bool>> CombinePredicates<T>(
        Expression<Func<T, bool>> first,
        Expression<Func<T, bool>> second)
    {
        var parameter = Expression.Parameter(typeof(T));

        var combined = Expression.AndAlso(
            Expression.Invoke(first, parameter),
            Expression.Invoke(second, parameter)
        );

        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }
}