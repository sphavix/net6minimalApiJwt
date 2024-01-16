using net6minimalApiJwt.Models;

namespace net6minimalApiJwt.Repositories
{
    public class UserRepository
    {
        public static List<User> Users = new()
        {
            new() { Username = "john_admin", EmailAddress = "john.admin@gmail.com",
                Password = "MyPass_w0rd", FirstName = "John", Surname = "Doe", Role = "Administrator"},
            new() { Username = "lucy_standard", EmailAddress = "john.standard@gmail.com",
                Password = "MyPass_w0rd", FirstName = "Lucy", Surname = "May", Role = "Standard"}
        };
    }
}
