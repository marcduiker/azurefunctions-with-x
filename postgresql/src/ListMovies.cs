using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MovieAPI;
using Npgsql;

namespace MovieApi
{
    public class ListMovies
    {
        private readonly ILogger _logger;

        public ListMovies(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ListMovies>();
        }

        [Function(nameof(ListMovies))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            HttpResponseData response = req.CreateResponse();
            var movies = new List<Movie>();

            var connectionString = Environment.GetEnvironmentVariable("PostgreSQLConnection");
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();
            await using (var cmd = new NpgsqlCommand("SELECT * FROM Movies ORDER BY release_year LIMIT 10", conn))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var id = reader.GetFieldValue<int>(0);
                    var title = reader.GetFieldValue<string>(1);
                    var releaseYear = reader.GetFieldValue<int>(2);
                    movies.Add(new Movie 
                    { 
                        Id = id,
                        Title = title,
                        ReleaseYear = releaseYear 
                    });
                }
            }

            await response.WriteAsJsonAsync(movies);
            response.StatusCode = HttpStatusCode.OK;

            return response;
        }
    }
}
