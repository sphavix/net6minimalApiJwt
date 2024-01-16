using net6minimalApiJwt.Models;
using net6minimalApiJwt.Repositories;

namespace net6minimalApiJwt.Services
{
    public class MovieService : IMovieService
    {
        public Movie Create(Movie movie)
        {
            movie.Id = MovieRepository.Movies.Count + 1;
            MovieRepository.Movies.Add(movie);
            return movie;
        }


        public Movie GetMovieById(int id)
        {
            var movie = MovieRepository.Movies.FirstOrDefault(x => x.Id == id);
            if (movie is null) return null;

            return movie;
        }

        public List<Movie> List()
        {
            var movies = MovieRepository.Movies;

            return movies.ToList();
        }

        public Movie Update(Movie movie)
        {
            var existingMovie = MovieRepository.Movies.FirstOrDefault(x => x.Id == movie.Id);

            if (existingMovie is null) return null;

            existingMovie.Title = movie.Title;
            existingMovie.Description = movie.Description;
            existingMovie.Rating = movie.Rating;

            return movie;
        }

        public bool Delete(int id)
        {
            var movie = MovieRepository.Movies.FirstOrDefault(x => x.Id == id);

            if (movie is null) return false;

            MovieRepository.Movies.Remove(movie);

            return true;
        }


    }
}
