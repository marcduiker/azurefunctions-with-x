using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MovieAPI;
using Npgsql;

namespace MovieApi
{
    public class CreateMovie
    {
        private readonly ILogger _logger;

        public CreateMovie(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<CreateMovie>();
        }

        [Function(nameof(CreateMovie))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            var movie = await req.ReadFromJsonAsync<Movie>();
            HttpResponseData response = req.CreateResponse();

            if (movie != null)
            {
                var connectionString = Environment.GetEnvironmentVariable("PostgreSQLConnection");
                await using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync();

                await using (var cmd = new NpgsqlCommand("INSERT INTO movies (title, release_year) VALUES (@title, @release_year)", conn))
                {
                    cmd.Parameters.AddWithValue("title",movie.Title);
                    cmd.Parameters.AddWithValue("release_year", movie.ReleaseYear);
                    await cmd.ExecuteNonQueryAsync();
                }
                response.StatusCode = HttpStatusCode.Accepted;
            }
            else 
            {
                await response.WriteStringAsync("No movie record was found in the body of the request.");
                response.StatusCode = HttpStatusCode.BadRequest;
            }

            return response;
        }
    }
}
