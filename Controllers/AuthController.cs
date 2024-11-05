using aps_sdk_authentication_sample.TokenHandlers;
using Microsoft.AspNetCore.Mvc;

namespace aps_sdk_authentication_sample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TokenHandler _autodeskOAuth;
        public AuthController(IConfiguration configuration)
        {
            _autodeskOAuth = new TokenHandler(configuration);
        }

        [HttpGet(Name = "Token")]
        public async Task<ActionResult<string>> Token()
        {
            try
            {
                var token = _autodeskOAuth.Login();
                string _token = token.ToString();
                return Ok(_token);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message.ToString());
            }
        }
    }

}
