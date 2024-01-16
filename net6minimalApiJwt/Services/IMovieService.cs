using net6minimalApiJwt.Models;

namespace net6minimalApiJwt.Services
{
    public interface IMovieService
    {
            public List<Movie> List();
            public Movie GetMovieById(int id);
            public Movie Create(Movie movie);
            public Movie Update(Movie movie);
            public bool Delete(int id);
    }
}
