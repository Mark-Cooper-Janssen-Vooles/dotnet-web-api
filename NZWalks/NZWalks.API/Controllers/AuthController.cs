using Microsoft.AspNetCore.Mvc;
using NZWalks.API.Models.Domain;
using NZWalks.API.Repositories;

namespace NZWalks.API.Controllers
{
    [ApiController]
    [Route("Auth")]
    public class AuthController : Controller
    {
        private readonly IUserRepository userRepository;
        private readonly ITokenHandler tokenHandler;

        public AuthController(IUserRepository userRepository, ITokenHandler tokenHandler)
        {
            this.userRepository = userRepository;
            this.tokenHandler = tokenHandler;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> LoginAsync(Models.DTO.LoginRequest loginRequest)
        {
            // validate the incoming request (done using fluent validator)

            // check if user is authenticated 
            var user = await userRepository.AuthenticateUserAsync(loginRequest.Username, 
                loginRequest.Password);

            if (user != null )
            {
                // generate a JWT token and send it back 
                var token = tokenHandler.CreateTokenAsync(user);
                return Ok(token);
            }

            return BadRequest("Username or password is incorrect.");

        }
    }
}
