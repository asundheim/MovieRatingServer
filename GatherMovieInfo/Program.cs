using MovieRating.Shared;
using MovieRatingShared;
using Services;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace GatherMovieInfo;

internal class Program
{

    const string RawIds = "";
    const string OutPath = ".\\movie-list.json";
    
    private const string movieFightClubID = "550";
    private const int startingPageNumber = 1;
    private const int totalPagesToFetch = 42;
    private const int minAverageVote = 4;
    private const int maxAverageVote = 7;
    private const int minVoteCount = 3000;
    private static Random rng = new Random();


    private static JsonSerializerOptions serializerOptions = new JsonSerializerOptions() { WriteIndented = true };

    public static async Task Main(string[] args)
    {
        TMDBService tmdbService = new TMDBService();
        //await GetNewMoviesFromListOfIDs(RawIds);
        //await UpdateBoxOfficRevenue();
        ShuffleMovieDatabase();
        //await UpdateReviewsForMovies();
        //PurgeUnwantedMovies();
    }


    private static async Task GetNewMoviesFromListOfIDs(string ids)
    {
        string[] splitIDs = RawIds.Split(',');
        int currentIndex = 0;
        for (int i = 0; i < splitIDs.Length; i++)
        {
            currentIndex = i;
            await GetNewMovieFromImdbID(splitIDs[i]);
            await Task.Delay(100);
        }
    }


    private static void ShuffleMovieDatabase()
    {
        RawMovieList db = JsonSerializer.Deserialize<RawMovieList>(File.ReadAllText(Constants.DBPath))!;
        List<RawMovie> movies = db.MovieDatabase;
        
        var shuffledMovies = movies.OrderBy(_  => rng.Next()).ToList();

        File.WriteAllText(Constants.DBPath, JsonSerializer.Serialize(shuffledMovies, serializerOptions));

    }

    private static async Task GetNewMovieFromImdbID(string imdbID)
    {
        TMDBService tmdbService = new TMDBService();
        OMDBService omdbService = new OMDBService();
        RawMovieList db = JsonSerializer.Deserialize<RawMovieList>(File.ReadAllText(Constants.DBPath))!;
        RawMovie? candidate = db.MovieDatabase.FirstOrDefault(x => x.imdbID == imdbID);
        if (candidate == null)
        {
            RawMovie omdbData = await omdbService.FetchOMDBDataFromIMDBID(imdbID);
            TMDBExternalDetailData tmdbData = await tmdbService.FetchMovieDataFromIMDBID(imdbID);
            List<WatchProvider> tmdbProviders = await tmdbService.GetWatchProvidersForId(tmdbData.TMDBID);
            List<string> tmdbReviews = await tmdbService.GetReviewsFromTmdb(tmdbData.TMDBID);

            RawMovie combinedData = omdbData;
            combinedData.TMDBId = tmdbData.TMDBID;
            combinedData.WatchProviders = tmdbProviders;
            combinedData.Reviews = tmdbReviews;

            db.MovieDatabase.Add(combinedData);

            Console.WriteLine($"{combinedData.Title} added to the movie database.");
        }
        else
        {
            Console.WriteLine($"{candidate.Title} already in database.");
            return;
        }

        File.WriteAllText(Constants.DBPath, JsonSerializer.Serialize(db, serializerOptions));
    }



    private static void PurgeUnwantedMovies()
    {
        RawMovieList db = JsonSerializer.Deserialize<RawMovieList>(File.ReadAllText(Constants.DBPath))!;
        List<RawMovie> toRemove = [];
        foreach (var rawMovie in db.MovieDatabase)
        {
            if (rawMovie.Ratings.Count <= 1)
            {
                Console.WriteLine($"{rawMovie.Title} to be removed from list, low rating source value");
                toRemove.Add(rawMovie);
            }

            if (rawMovie.Poster == "N/A")
            {
                Console.WriteLine($"{rawMovie.Title} to be removed from list, missing poster");
                toRemove.Add(rawMovie);
            }
        }

        foreach (var movieToRemove in toRemove)
        {
            db.MovieDatabase.Remove(movieToRemove);
        }


        //Console.WriteLine($"{toRemove.Count} Movies to Remove");
        File.WriteAllText(Constants.DBPath, JsonSerializer.Serialize(db, serializerOptions));
    }

    
    private static void PurgeOldMovies()
    {
        RawMovieList db = JsonSerializer.Deserialize<RawMovieList>(File.ReadAllText(Constants.DBPath))!;
        List<RawMovie> toRemove = [];
        foreach (var rawMovie in db.MovieDatabase)
        {
            if (rawMovie.TMDBId == 0)
            {
                toRemove.Add(rawMovie);
            }
        }

        foreach (var movieToRemove in toRemove)
        {
            db.MovieDatabase.Remove(movieToRemove);
        }

        File.WriteAllText(Constants.DBPath, JsonSerializer.Serialize(db, serializerOptions));
    }



    private static async Task UpdateWatchProviders()
    {
        TMDBService tmdbService = new TMDBService();
        RawMovieList db = JsonSerializer.Deserialize<RawMovieList>(File.ReadAllText(Constants.DBPath))!;

        foreach (var movie in db.MovieDatabase)
        {
            if (movie.TMDBId != 0 && movie.WatchProviders is null)
            {
                List<WatchProvider> providers = await tmdbService.GetWatchProvidersForId(movie.TMDBId);
                movie.WatchProviders = providers;
                Console.WriteLine($"Added providers for {movie.Title}");
            }
            else
            {
                if (movie.TMDBId == 0)
                {
                    Console.WriteLine($"Skipping {movie.Title} - no TMDBId");
                }
                else
                {
                    Console.WriteLine($"Skipping {movie.Title} - already populated");
                }
            }

            await Task.Delay(50);
        }

        File.WriteAllText(Constants.DBPath, JsonSerializer.Serialize(db, serializerOptions));
    }

    private static async Task UpdateReviewsForMovies()
    {
        TMDBService tmdbService = new TMDBService();
        RawMovieList db = JsonSerializer.Deserialize<RawMovieList>(File.ReadAllText(Constants.DBPath))!;

        foreach (var movie in db.MovieDatabase)
        {
            if (movie.TMDBId != 0 && movie.Reviews is null)
            {
                await tmdbService.GetReviewsFromTmdb(movie.TMDBId);
                List<string> reviews = await tmdbService.GetReviewsFromTmdb(movie.TMDBId);
                movie.Reviews = reviews;
                Console.WriteLine($"Added reviews for {movie.Title}");
            }
            else
            {
                if (movie.TMDBId == 0)
                {
                    Console.WriteLine($"Skipping {movie.Title} - no TMDBId");
                }
                else
                {
                    Console.WriteLine($"Skipping {movie.Title} - already populated");
                }
            }

            await Task.Delay(50);
        }

        File.WriteAllText(Constants.DBPath, JsonSerializer.Serialize(db, serializerOptions));
    }


    private static async Task UpdateBoxOfficRevenue()
    {
        TMDBService tmdbService = new TMDBService();
        RawMovieList db = JsonSerializer.Deserialize<RawMovieList>(File.ReadAllText(Constants.DBPath))!;

        foreach (var movie in db.MovieDatabase)
        {
            if (movie.TMDBId != 0 && movie.BoxOffice == "N/A")
            {
                int revenue = (await tmdbService.GetRevenueInfoFromDB(movie.TMDBId));
                if (revenue > 0)
                {
                    string stringRevenue = Convert.ToString(revenue);
                    movie.BoxOffice = stringRevenue;
                    Console.WriteLine($"Added revenue for {movie.Title}");
                }
            }
            if (movie.BoxOffice == "0")
            {
                movie.BoxOffice = "N/A";
                Console.WriteLine($"Updating 0 Rev to No Data for {movie.Title}");
            }
            else
            {
                if (movie.TMDBId == 0)
                {
                    Console.WriteLine($"Skipping {movie.Title} - no TMDBId");
                }
                else
                {
                    Console.WriteLine($"Skipping {movie.Title} - already populated");
                }
            }

            await Task.Delay(50);
        }

        File.WriteAllText(Constants.DBPath, JsonSerializer.Serialize(db, serializerOptions));
    }


    private static async Task UpdateTMDBIds()
    {
        TMDBService tmdbService = new TMDBService();
        RawMovieList db = JsonSerializer.Deserialize<RawMovieList>(File.ReadAllText(Constants.DBPath))!;
        var newDb = JsonSerializer.Deserialize<NewMovieDatabase>(File.ReadAllText(Constants.NewDBPath))!;

        foreach (var newMovie in newDb.NewMovies)
        {
            string imdbId = await tmdbService.GetIMDbIdFromTMDbId(newMovie.Id);

            RawMovie? existingMovie = db.MovieDatabase.FirstOrDefault(r => r.imdbID == imdbId);
            if (existingMovie is not null)
            {
                existingMovie.TMDBId = newMovie.Id;
                Console.WriteLine($"Updated {existingMovie.Title}");
            }
            else
            {
                Console.WriteLine($"Skipped {newMovie.Title} - not found in DB");
            }

            await Task.Delay(50);
        }

        File.WriteAllText(Constants.DBPath, JsonSerializer.Serialize(db, serializerOptions));
    }
}
