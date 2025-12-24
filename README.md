# Film Database - Project .NET Framework

* Naam: Romeo Weyns
* Studentennummer: 0177605-95
* Academiejaar: 25-26
* Klasgroep: INF-202A
* Onderwerp: Film, Actor, FilmDirector

## Sprint 1 

### UML Class Diagram

```mermaid
classDiagram
    class Actor {
        -List~Film~ _films
        +string Name
        +string Nationality
        +DateTime DateOfBirth
        +int? Age
        +IReadOnlyList~Film~ Films
        +AddFilm(Film film)
        +RemoveFilm(Film film)
        +ClearFilms()
        +AddFilmsRange(IEnumerable~Film~ films)
        +ToString() string
        -CalculateAge() int
    }

    class Film {
        -List~Actor~ _actors
        +string Title
        +Genre Genre
        +DateTime ReleaseDate
        +double Rating
        +FilmDirector Director
        +IReadOnlyList~Actor~ Actors
        +AddActor(Actor actor)
        +RemoveActor(Actor actor)
        +ClearActors()
        +AddActorsRange(IEnumerable~Actor~ actors)
        +ToString() string
    }

    class FilmDirector {
        -List~Film~ _films
        +string Name
        +string Country
        +DateTime CareerStart
        +IReadOnlyList~Film~ Films
        +AddFilm(Film film)
        +RemoveFilm(Film film)
        +ClearFilms()
        +AddFilmsRange(IEnumerable~Film~ films)
        +ToString() string
    }

    class Genre {
        <<enumeration>>
        Action
        Comedy
        Drama
        Horror
        Romance
        SciFi
        Thriller
        Documentary
    }

    Actor "0..*" -- "0..*" Film : appears in
    Film "0..*" --> "1" FilmDirector : directed by
    FilmDirector "1" -- "0..*" Film : directs
    Film --> Genre : has
```

## Sprint 3

### Beide zoekcriteria ingevuld

```sql
SELECT "a"."ImdbId", "a"."DateOfBirth", "a"."DateOfDeath", "a"."Name", "a"."Nationality"
FROM "Actors" AS "a"
WHERE instr(upper("a"."Name"), @__upperFilter_0) > 0 AND "a"."DateOfBirth" <= @__maxDateOfBirth_1
-- Name = "Emma" AND MinimumAge = 20
```

### Enkel zoeken op naam

```sql
SELECT "a"."ImdbId", "a"."DateOfBirth", "a"."DateOfDeath", "a"."Name", "a"."Nationality"
FROM "Actors" AS "a"
WHERE instr(upper("a"."Name"), @__upperFilter_0) > 0
-- Name = "Emma"
```

### Enkel zoeken op minimum leeftijd

```sql
SELECT "a"."ImdbId", "a"."DateOfBirth", "a"."DateOfDeath", "a"."Name", "a"."Nationality"
FROM "Actors" AS "a"
WHERE "a"."DateOfBirth" <= @__maxDateOfBirth_0
-- MinimumAge = 20
```

### Beide zoekcriteria leeg

```sql
SELECT "a"."ImdbId", "a"."DateOfBirth", "a"."DateOfDeath", "a"."Name", "a"."Nationality"
FROM "Actors" AS "a"
```

## Sprint 4

```mermaid
classDiagram
    class Actor {
        +Guid ImdbId
        +string Name
        +string Nationality
        +DateTime DateOfBirth
        +DateTime? DateOfDeath
        +int Age
        +ICollection~ActorFilm~ ActorFilms
        +Validate(ValidationContext) IEnumerable~ValidationResult~
        -CalculateAge(DateTime, DateTime?) int
    }

    class Film {
        +Guid ImdbId
        +string Title
        +Genre Genre
        +DateTime ReleaseDate
        +double Rating
        +FilmDirector Director
        +ICollection~ActorFilm~ ActorFilms
    }

    class ActorFilm {
        +Actor Actor
        +Film Film
        +int? ScreenTime
    }

    class FilmDirector {
        +Guid ImdbId
        +string Name
        +string Country
        +int? CareerStart
        +int? CareerEnd
        +ICollection~Film~ Films
        +Validate(ValidationContext) IEnumerable~ValidationResult~
    }

    class Genre {
        <<enumeration>>
        None
        Action
        Comedy
        Drama
        Horror
        Romance
        SciFi
        Thriller
        Documentary
        Western
        Musical
        Animation
        Fantasy
        Short
    }

    Actor "1" -- "0..*" ActorFilm : has
    Film "1" -- "0..*" ActorFilm : has
    ActorFilm --> Actor : references
    ActorFilm --> Film : references
    Film "0..*" --> "1" FilmDirector : directed by
    FilmDirector "1" -- "0..*" Film : directs
    Film --> Genre : has
```

## Sprint 6

### New Director

#### Request

```http
POST http://localhost:5199/api/directors
Accept: application/json
Content-Type: application/json

{
  "name": "Romeo Weyns",
  "country": "Belgium",
  "careerStart": 1995,
  "careerEnd": null
}
```

#### Response

```http
HTTP/1.1 201 Created
Content-Type: application/json; charset=utf-8
Date: Wed, 24 Dec 2025 19:43:07 GMT
Server: Kestrel
Location: http://localhost:5199/api/Directors/04d45648-c449-4727-9314-938cbf8e8c6f
Transfer-Encoding: chunked

{
  "imdbId": "04d45648-c449-4727-9314-938cbf8e8c6f",
  "name": "Romeo Weyns",
  "country": "Belgium",
  "careerStart": 1995,
  "careerEnd": null
}
```
---