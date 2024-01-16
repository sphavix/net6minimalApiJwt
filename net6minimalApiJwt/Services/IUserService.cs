using net6minimalApiJwt.Models;

namespace net6minimalApiJwt.Services
{
    public interface IUserService
    {
        public User Get(UserLogin userLogin);
    }
}
