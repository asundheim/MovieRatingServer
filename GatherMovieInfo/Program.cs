using MovieRating.Shared;
using MovieRatingShared;
using Services;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Xml;

namespace GatherMovieInfo;

internal partial class Program
{

    const string RawIds = "tt35291758";
    
    private const string movieFightClubID = "550";
    private const int startingPageNumber = 1;
    private const int totalPagesToFetch = 42;
    private const int minAverageVote = 4;
    private const int maxAverageVote = 7;
    private const int minVoteCount = 3000;
    private static Random rng = new Random();
    private static string regexWebsites = "(FULL SPOILER-FREE REVIEW)|((((https:\\/\\/)www\\.)\\w+)\\.\\w{3})|((?<=.\\w{3})\\/\\S+(\\b))|(www\\.\\S+\\.\\w{3})";
    private static string regexRatings = "((\\d+.\\d+|\\d+)\\/\\d+)|((GRADE|RATING|Grade|Rating|Score|SCORE)[:]\\s)(\\w+)|((Verdict|VERDICT)\\:\\s\\w+)|(\\d+|\\d+\\,\\d+|\\d+.\\d+|\\d+-)((\\s\\w+\\s|\\s)(out of|Out Of)\\s)(\\d+,\\d+|\\d+|\\d+\\.\\d+)|(\\s|\\n|\\r)[A-Z](\\-|\\+)|(★+|★+½)|(\\u2605)";

    private static JsonSerializerOptions serializerOptions = new JsonSerializerOptions() { WriteIndented = true };

    [GeneratedRegex(@"((\d+.\d+|\d+)\/\d+)|((GRADE|RATING|Grade|Rating|Score|SCORE)[:]\s)(\w+)|((Verdict|VERDICT)\:\s\w+)|(\d+|\d+\,\d+|\d+.\d+|\d+-)((\s\w+\s|\s)(out of|Out Of)\s)(\d+,\d+|\d+|\d+\.\d+)|(\s|\n|\r)[A-Z](\-|\+)|(★+|★+½)|(\\u2605)", RegexOptions.Compiled)]
    private static partial Regex _reviewRegex();

    public static async Task Main(string[] args)
    {
        TMDBService tmdbService = new TMDBService();
        await GetNewMoviesFromListOfIDs(RawIds);
        //await UpdateBoxOfficRevenue();
        //ShuffleMovieDatabase();
        //GetListOfStreamProviders();
        //await UpdateReviewsForMovies();
        //PurgeUnwantedMovies();
        //CleanUpWatchProviders();
        //ReviewRatingsRegex();
        //ConvertMoneyStrings();
        //ReviewWebsiteRegex();
    }

    //--------------------------------------------------------------------
    private static void CleanUpWatchProviders()
    {
        RawMovieList db = JsonSerializer.Deserialize<RawMovieList>(File.ReadAllText(Constants.DBPath))!;
        foreach (RawMovie movie in db.MovieDatabase)
        {
            movie.WatchProviders = movie.WatchProviders?
                .Where(p => !p.ProviderName.Contains("Amazon Channel") &&
                            !p.ProviderName.Contains("Apple TV Channel") &&
                            !p.ProviderName.Contains("with Ads") &&
                            !p.ProviderName.Contains("Roku Premium Channel") &&
                            !p.ProviderName.Contains("Plus Essential")).ToList();
        }
        File.WriteAllText(Constants.DBPath, JsonSerializer.Serialize(db, serializerOptions));
    }

    private static void ReviewRatingsRegex()
    {
        RawMovieList db = JsonSerializer.Deserialize<RawMovieList>(File.ReadAllText(Constants.DBPath))!;
        foreach (RawMovie movie in db.MovieDatabase)
        {
            if (movie.Reviews != null && movie.Reviews.Count > 0)
            {
                for (int i = 0; i < movie.Reviews.Count; i++)
                {
                    string? original = movie.Reviews[i];
                    string alteredReview = _reviewRegex().Replace(original, (Match m) => new string('*', m.Length));
                    movie.Reviews[i] = alteredReview;
                }
            }
        }
        File.WriteAllText(Constants.DBPath, JsonSerializer.Serialize(db, serializerOptions));
    }

    private static void ConvertMoneyStrings()
    {
        RawMovieList db = JsonSerializer.Deserialize<RawMovieList>(File.ReadAllText(Constants.DBPath))!;
        foreach (RawMovie movie in db.MovieDatabase)
        {
            if (movie.BoxOffice != "N/A")
            {
                string tempBoxOffice = movie.BoxOffice;
                int intBoxOffice = int.Parse(tempBoxOffice, NumberStyles.Currency);
                string convertedBoxOffice = intBoxOffice.ToString("C0");
                movie.BoxOffice = convertedBoxOffice;
            }
        }
        File.WriteAllText(Constants.DBPath, JsonSerializer.Serialize(db, serializerOptions));
    }

    private static void ReviewWebsiteRegex()
    {
        RawMovieList db = JsonSerializer.Deserialize<RawMovieList>(File.ReadAllText(Constants.DBPath))!;
        foreach (RawMovie movie in db.MovieDatabase)
        {
            if (movie.Reviews != null && movie.Reviews.Count > 0)
            {
                List<string> newReviews = new List<string>();
                for (int i = 0; i < movie.Reviews.Count; i++)
                {
                    string? original = movie.Reviews[i];
                    bool matchingReview = _reviewRegex().IsMatch(original);
                    if (!matchingReview)
                    {
                        newReviews.Add(original);
                    }
                }
                movie.Reviews = newReviews;
            }
        }
        File.WriteAllText(Constants.DBPath, JsonSerializer.Serialize(db, serializerOptions));
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

    private static void GetListOfStreamProviders()
    {
        RawMovieList db = JsonSerializer.Deserialize<RawMovieList>(File.ReadAllText(Constants.DBPath))!;
        List<RawMovie> movies = db.MovieDatabase;
        List<string> streamProviders = new List<string>();

        foreach (RawMovie movie in movies)
        {
            if (movie.WatchProviders != null)
            {
                if (movie.WatchProviders.Count > 0)
                {
                    for (int i = 0; i < movie.WatchProviders.Count; i++)
                    {
                        string watchProvider = movie.WatchProviders[i].ProviderName;
                        if (!streamProviders.Contains(watchProvider))
                        {
                            streamProviders.Add(watchProvider);
                            Console.WriteLine(watchProvider);
                        }
                    }
                }
            }
        }

        Console.WriteLine(streamProviders);
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

            if (combinedData.BoxOffice == "N/A" && combinedData.TMDBId != 0)
            {
                int updateBoxOffice = (await tmdbService.GetRevenueInfoFromDB(combinedData.TMDBId));
                if (updateBoxOffice > 0)
                {
                    string stringRevenue = Convert.ToString(updateBoxOffice);
                    int intBoxOffice = int.Parse(stringRevenue, NumberStyles.Currency);
                    string convertedBoxOffice = intBoxOffice.ToString("C0");
                    combinedData.BoxOffice = convertedBoxOffice;
                }
            }

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
