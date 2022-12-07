using NZWalks.API.Models.Domain;

namespace NZWalks.API.Repositories
{
    public class StaticUserRepository : IUserRepository
    {
        private List<User> Users = new List<User>()
        {
            new User()
            {
                Id = Guid.NewGuid(),
                Username= "Readonly",
                Email="Readonly@test.com",
                Password="password",
                // Roles= new List<string> { "reader" },
                FirstName= "Readonly",
                LastName= "Readonly"
            },
            new User()
            {
                Id = Guid.NewGuid(),
                Username= "Admin",
                Email="Admin@test.com",
                Password="password",
                // Roles= new List<string> { "reader", "writer" },
                FirstName= "Admin",
                LastName= "Admin"
            },
        };

        public async Task<User> AuthenticateUserAsync(string username, string password)
        {
            var user = Users.Find(x => x.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase) &&
            x.Password == password);

            return user;
        }
    }
}
