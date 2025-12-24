using FilmManagement.BL.Domain;

namespace FilmManagement.DAL.EF;

public static class DataSeeder
{
    public static void Seed(FilmDbContext context)
    {
        // === DIRECTORS ===
        var lanthimos = new Director 
        { 
            Name = "Yorgos Lanthimos", 
            Country = "Greece", 
            CareerStart = 2001
        };
        
        var kubrick = new Director 
        { 
            Name = "Stanley Kubrick", 
            Country = "USA", 
            CareerStart = 1951,
            CareerEnd = 1999
        };
        
        var lynch = new Director 
        { 
            Name = "David Lynch", 
            Country = "USA", 
            CareerStart = 1970,
            CareerEnd = 2006
        };
        
        var eggers = new Director 
        { 
            Name = "Robert Eggers", 
            Country = "USA", 
            CareerStart = 2015
        };

        var godard = new Director
        {
            Name = "Jean-Luc Godard",
            Country = "France",
            CareerStart = 1950,
            CareerEnd = 2018
        };

        var nolan = new Director
        {
            Name = "Christopher Nolan",
            Country = "UK",
            CareerStart = 1998
        };

        var villeneuve = new Director
        {
            Name = "Denis Villeneuve",
            Country = "Canada",
            CareerStart = 1998
        };

        var anderson = new Director
        {
            Name = "Wes Anderson",
            Country = "USA",
            CareerStart = 1996
        };

        var tarantino = new Director
        {
            Name = "Quentin Tarantino",
            Country = "USA",
            CareerStart = 1992
        };

        context.Directors.AddRange(lanthimos, kubrick, lynch, eggers, godard, 
            nolan, villeneuve, anderson, tarantino);

        // === ACTORS ===
        var stone = new Actor
        {
            Name = "Emma Stone",
            Nationality = "American",
            DateOfBirth = new DateTime(1988, 11, 6)
        };
        
        var farrell = new Actor
        {
            Name = "Colin Farrell",
            Nationality = "Irish",
            DateOfBirth = new DateTime(1976, 5, 31),
        };
        
        var kidman = new Actor
        {
            Name = "Nicole Kidman",
            Nationality = "Australian",
            DateOfBirth = new DateTime(1967, 6, 20),
        };
        
        var pattinson = new Actor
        {
            Name = "Robert Pattinson",
            Nationality = "British",
            DateOfBirth = new DateTime(1986, 5, 13),
        };
        
        var dafoe = new Actor
        {
            Name = "Willem Dafoe",
            Nationality = "American",
            DateOfBirth = new DateTime(1955, 7, 22),
        };
        
        var keoghan = new Actor
        {
            Name = "Barry Keoghan",
            Nationality = "Irish",
            DateOfBirth = new DateTime(1992, 10, 18),
        };
        
        var belmondo = new Actor
        {
            Name = "Jean-Paul Belmondo",
            Nationality = "French",
            DateOfBirth = new DateTime(1933, 4, 9),
            DateOfDeath = new DateTime(2021, 9, 6)
        };

        var karina = new Actor
        {
            Name = "Anna Karina",
            Nationality = "Danish-French",
            DateOfBirth = new DateTime(1940, 9, 22),
            DateOfDeath = new DateTime(2019, 12, 14)
        };

        var weisz = new Actor
        {
            Name = "Rachel Weisz",
            Nationality = "British",
            DateOfBirth = new DateTime(1970, 3, 7)
        };

        var hoult = new Actor
        {
            Name = "Nicholas Hoult",
            Nationality = "British",
            DateOfBirth = new DateTime(1989, 12, 7)
        };

        var leigh = new Actor
        {
            Name = "Jennifer Jason Leigh",
            Nationality = "American",
            DateOfBirth = new DateTime(1962, 2, 5)
        };

        var dern = new Actor
        {
            Name = "Laura Dern",
            Nationality = "American",
            DateOfBirth = new DateTime(1967, 2, 10)
        };

        var chalamet = new Actor
        {
            Name = "Timothée Chalamet",
            Nationality = "American",
            DateOfBirth = new DateTime(1995, 12, 27)
        };

        var zendaya = new Actor
        {
            Name = "Zendaya",
            Nationality = "American",
            DateOfBirth = new DateTime(1996, 9, 1)
        };

        var fiennes = new Actor
        {
            Name = "Ralph Fiennes",
            Nationality = "British",
            DateOfBirth = new DateTime(1962, 12, 22)
        };

        var swinton = new Actor
        {
            Name = "Tilda Swinton",
            Nationality = "British",
            DateOfBirth = new DateTime(1960, 11, 5)
        };

        var travolta = new Actor
        {
            Name = "John Travolta",
            Nationality = "American",
            DateOfBirth = new DateTime(1954, 2, 18)
        };

        var thurman = new Actor
        {
            Name = "Uma Thurman",
            Nationality = "American",
            DateOfBirth = new DateTime(1970, 4, 29)
        };

        var jackson = new Actor
        {
            Name = "Samuel L. Jackson",
            Nationality = "American",
            DateOfBirth = new DateTime(1948, 12, 21)
        };

        context.Actors.AddRange(stone, farrell, kidman, pattinson, dafoe, keoghan, 
            belmondo, karina, weisz, hoult, leigh, dern, chalamet, zendaya, 
            fiennes, swinton, travolta, thurman, jackson);

        // === FILMS ===
        
        // Lanthimos films
        var theLobster = new Film
        {
            Title = "The Lobster",
            Genre = Genre.Drama | Genre.Romance,
            ReleaseDate = new DateTime(2015, 10, 16),
            Rating = 8.2,
            Director = lanthimos
        };
        
        var poorThings = new Film
        {
            Title = "Poor Things",
            Genre = Genre.Comedy | Genre.Romance | Genre.SciFi,
            ReleaseDate = new DateTime(2023, 12, 8),
            Rating = 7.9,
            Director = lanthimos
        };

        var theKillingOfASacredDeer = new Film
        {
            Title = "The Killing of a Sacred Deer",
            Genre = Genre.Drama | Genre.Thriller,
            ReleaseDate = new DateTime(2017, 10, 20),
            Rating = 8.0,
            Director = lanthimos
        };

        var theFavourite = new Film
        {
            Title = "The Favourite",
            Genre = Genre.Drama | Genre.Comedy,
            ReleaseDate = new DateTime(2018, 11, 23),
            Rating = 7.5,
            Director = lanthimos
        };
        
        // Kubrick films
        var theShining = new Film
        {
            Title = "The Shining",
            Genre = Genre.Horror | Genre.Thriller,
            ReleaseDate = new DateTime(1980, 5, 23),
            Rating = 8.4,
            Director = kubrick
        };

        var aClockworkOrange = new Film
        {
            Title = "A Clockwork Orange",
            Genre = Genre.Drama | Genre.SciFi,
            ReleaseDate = new DateTime(1971, 12, 19),
            Rating = 8.3,
            Director = kubrick
        };

        var fullMetalJacket = new Film
        {
            Title = "Full Metal Jacket",
            Genre = Genre.Drama | Genre.Action,
            ReleaseDate = new DateTime(1987, 6, 26),
            Rating = 8.3,
            Director = kubrick
        };
        
        // Lynch films
        var mulhollandDrive = new Film
        {
            Title = "Mulholland Drive",
            Genre = Genre.Thriller | Genre.Drama,
            ReleaseDate = new DateTime(2001, 10, 12),
            Rating = 7.9,
            Director = lynch
        };

        var blueVelvet = new Film
        {
            Title = "Blue Velvet",
            Genre = Genre.Thriller | Genre.Drama,
            ReleaseDate = new DateTime(1986, 9, 19),
            Rating = 7.7,
            Director = lynch
        };
        
        // Eggers films
        var theLighthouse = new Film
        {
            Title = "The Lighthouse",
            Genre = Genre.Horror | Genre.Thriller | Genre.Drama,
            ReleaseDate = new DateTime(2019, 10, 18),
            Rating = 7.4,
            Director = eggers
        };
        
        var theNorthman = new Film
        {
            Title = "The Northman",
            Genre = Genre.Action | Genre.Drama,
            ReleaseDate = new DateTime(2022, 4, 22),
            Rating = 7.0,
            Director = eggers
        };

        var theWitch = new Film
        {
            Title = "The Witch",
            Genre = Genre.Horror | Genre.Drama,
            ReleaseDate = new DateTime(2015, 1, 27),
            Rating = 7.0,
            Director = eggers
        };
        
        // Godard films
        var breathless = new Film
        {
            Title = "À bout de souffle (Breathless)",
            Genre = Genre.Drama | Genre.Romance,
            ReleaseDate = new DateTime(1960, 3, 16),
            Rating = 7.8,
            Director = godard
        };

        var vivreSaVie = new Film
        {
            Title = "Vivre sa vie",
            Genre = Genre.Drama,
            ReleaseDate = new DateTime(1962, 9, 20),
            Rating = 8.0,
            Director = godard
        };

        var pierrotLeFou = new Film
        {
            Title = "Pierrot le Fou",
            Genre = Genre.Drama | Genre.Romance,
            ReleaseDate = new DateTime(1965, 11, 5),
            Rating = 7.5,
            Director = godard
        };

        // Nolan films
        var inception = new Film
        {
            Title = "Inception",
            Genre = Genre.Action | Genre.SciFi | Genre.Thriller,
            ReleaseDate = new DateTime(2010, 7, 16),
            Rating = 7.8,
            Director = nolan
        };

        var theDarkKnight = new Film
        {
            Title = "The Dark Knight",
            Genre = Genre.Action | Genre.Drama,
            ReleaseDate = new DateTime(2008, 7, 18),
            Rating = 8.5,
            Director = nolan
        };

        var interstellar = new Film
        {
            Title = "Interstellar",
            Genre = Genre.SciFi | Genre.Drama,
            ReleaseDate = new DateTime(2014, 11, 7),
            Rating = 7.7,
            Director = nolan
        };

        // Villeneuve films
        var dune = new Film
        {
            Title = "Dune",
            Genre = Genre.SciFi | Genre.Drama | Genre.Action,
            ReleaseDate = new DateTime(2021, 10, 22),
            Rating = 8.0,
            Director = villeneuve
        };

        var bladeRunner2049 = new Film
        {
            Title = "Blade Runner 2049",
            Genre = Genre.SciFi | Genre.Thriller,
            ReleaseDate = new DateTime(2017, 10, 6),
            Rating = 8.0,
            Director = villeneuve
        };

        var arrival = new Film
        {
            Title = "Arrival",
            Genre = Genre.SciFi | Genre.Drama,
            ReleaseDate = new DateTime(2016, 11, 11),
            Rating = 7.9,
            Director = villeneuve
        };

        // Wes Anderson films
        var theGrandBudapestHotel = new Film
        {
            Title = "The Grand Budapest Hotel",
            Genre = Genre.Comedy | Genre.Drama,
            ReleaseDate = new DateTime(2014, 3, 28),
            Rating = 8.1,
            Director = anderson
        };

        var theRoyalTenenbaums = new Film
        {
            Title = "The Royal Tenenbaums",
            Genre = Genre.Comedy | Genre.Drama,
            ReleaseDate = new DateTime(2001, 12, 14),
            Rating = 7.6,
            Director = anderson
        };

        // Tarantino films
        var pulpFiction = new Film
        {
            Title = "Pulp Fiction",
            Genre = Genre.Drama,
            ReleaseDate = new DateTime(1994, 10, 14),
            Rating = 8.9,
            Director = tarantino
        };

        var killBill = new Film
        {
            Title = "Kill Bill: Vol. 1",
            Genre = Genre.Action | Genre.Thriller,
            ReleaseDate = new DateTime(2003, 10, 10),
            Rating = 8.2,
            Director = tarantino
        };

        context.Films.AddRange(
            theLobster, poorThings, theKillingOfASacredDeer, theFavourite,
            theShining, aClockworkOrange, fullMetalJacket,
            mulhollandDrive, blueVelvet,
            theLighthouse, theNorthman, theWitch,
            breathless, vivreSaVie, pierrotLeFou,
            inception, theDarkKnight, interstellar,
            dune, bladeRunner2049, arrival,
            theGrandBudapestHotel, theRoyalTenenbaums,
            pulpFiction, killBill
        );

        // === ACTOR-FILM RELATIONSHIPS WITH SCREEN TIME ===
        
        // Lanthimos films
        theLobster.ActorFilms.Add(new ActorFilm { Actor = farrell, Film = theLobster, ScreenTime = 95 });
        theLobster.ActorFilms.Add(new ActorFilm { Actor = weisz, Film = theLobster, ScreenTime = 60 });

        poorThings.ActorFilms.Add(new ActorFilm { Actor = stone, Film = poorThings, ScreenTime = 110 });
        poorThings.ActorFilms.Add(new ActorFilm { Actor = dafoe, Film = poorThings, ScreenTime = 45 });
        poorThings.ActorFilms.Add(new ActorFilm { Actor = weisz, Film = poorThings, ScreenTime = 35 });

        theKillingOfASacredDeer.ActorFilms.Add(new ActorFilm { Actor = farrell, Film = theKillingOfASacredDeer, ScreenTime = 100 });
        theKillingOfASacredDeer.ActorFilms.Add(new ActorFilm { Actor = kidman, Film = theKillingOfASacredDeer, ScreenTime = 85 });
        theKillingOfASacredDeer.ActorFilms.Add(new ActorFilm { Actor = keoghan, Film = theKillingOfASacredDeer, ScreenTime = 75 });

        theFavourite.ActorFilms.Add(new ActorFilm { Actor = stone, Film = theFavourite, ScreenTime = 95 });
        theFavourite.ActorFilms.Add(new ActorFilm { Actor = weisz, Film = theFavourite, ScreenTime = 90 });

        // Kubrick films
        aClockworkOrange.ActorFilms.Add(new ActorFilm { Actor = dafoe, Film = aClockworkOrange, ScreenTime = null });

        // Lynch films
        mulhollandDrive.ActorFilms.Add(new ActorFilm { Actor = kidman, Film = mulhollandDrive, ScreenTime = 110 });
        mulhollandDrive.ActorFilms.Add(new ActorFilm { Actor = dern, Film = mulhollandDrive, ScreenTime = 40 });

        blueVelvet.ActorFilms.Add(new ActorFilm { Actor = dern, Film = blueVelvet, ScreenTime = 95 });
        blueVelvet.ActorFilms.Add(new ActorFilm { Actor = leigh, Film = blueVelvet, ScreenTime = 30 });

        // Eggers films
        theLighthouse.ActorFilms.Add(new ActorFilm { Actor = pattinson, Film = theLighthouse, ScreenTime = 105 });
        theLighthouse.ActorFilms.Add(new ActorFilm { Actor = dafoe, Film = theLighthouse, ScreenTime = 105 });

        theNorthman.ActorFilms.Add(new ActorFilm { Actor = kidman, Film = theNorthman, ScreenTime = 55 });
        theNorthman.ActorFilms.Add(new ActorFilm { Actor = dafoe, Film = theNorthman, ScreenTime = 30 });
        theNorthman.ActorFilms.Add(new ActorFilm { Actor = swinton, Film = theNorthman, ScreenTime = 25 });

        theWitch.ActorFilms.Add(new ActorFilm { Actor = dafoe, Film = theWitch, ScreenTime = 40 });

        // Godard films
        breathless.ActorFilms.Add(new ActorFilm { Actor = belmondo, Film = breathless, ScreenTime = 80 });
        
        vivreSaVie.ActorFilms.Add(new ActorFilm { Actor = karina, Film = vivreSaVie, ScreenTime = null });

        pierrotLeFou.ActorFilms.Add(new ActorFilm { Actor = belmondo, Film = pierrotLeFou, ScreenTime = 90 });
        pierrotLeFou.ActorFilms.Add(new ActorFilm { Actor = karina, Film = pierrotLeFou, ScreenTime = 85 });

        // Nolan films
        inception.ActorFilms.Add(new ActorFilm { Actor = farrell, Film = inception, ScreenTime = 30 });
        inception.ActorFilms.Add(new ActorFilm { Actor = pattinson, Film = inception, ScreenTime = 15 });

        theDarkKnight.ActorFilms.Add(new ActorFilm { Actor = pattinson, Film = theDarkKnight, ScreenTime = 120 });

        interstellar.ActorFilms.Add(new ActorFilm { Actor = chalamet, Film = interstellar, ScreenTime = 35 });

        // Villeneuve films
        dune.ActorFilms.Add(new ActorFilm { Actor = chalamet, Film = dune, ScreenTime = 130 });
        dune.ActorFilms.Add(new ActorFilm { Actor = zendaya, Film = dune, ScreenTime = 40 });

        bladeRunner2049.ActorFilms.Add(new ActorFilm { Actor = pattinson, Film = bladeRunner2049, ScreenTime = 105 });

        arrival.ActorFilms.Add(new ActorFilm { Actor = leigh, Film = arrival, ScreenTime = 60 });

        // Wes Anderson films
        theGrandBudapestHotel.ActorFilms.Add(new ActorFilm { Actor = fiennes, Film = theGrandBudapestHotel, ScreenTime = 100 });
        theGrandBudapestHotel.ActorFilms.Add(new ActorFilm { Actor = swinton, Film = theGrandBudapestHotel, ScreenTime = 15 });
        theGrandBudapestHotel.ActorFilms.Add(new ActorFilm { Actor = dafoe, Film = theGrandBudapestHotel, ScreenTime = 20 });

        theRoyalTenenbaums.ActorFilms.Add(new ActorFilm { Actor = hoult, Film = theRoyalTenenbaums, ScreenTime = 45 });

        // Tarantino films
        pulpFiction.ActorFilms.Add(new ActorFilm { Actor = travolta, Film = pulpFiction, ScreenTime = 105 });
        pulpFiction.ActorFilms.Add(new ActorFilm { Actor = thurman, Film = pulpFiction, ScreenTime = 85 });
        pulpFiction.ActorFilms.Add(new ActorFilm { Actor = jackson, Film = pulpFiction, ScreenTime = 95 });

        killBill.ActorFilms.Add(new ActorFilm { Actor = thurman, Film = killBill, ScreenTime = 110 });
        killBill.ActorFilms.Add(new ActorFilm { Actor = dern, Film = killBill, ScreenTime = 25 });
        
        context.SaveChanges();
        context.ChangeTracker.Clear();
    }
}