using Microsoft.AspNetCore.Mvc;

namespace WinDir.Client.Controllers
{
    [Route("values")]
    [ApiController]
    public class ValuesController : ControllerBase
    {

        [HttpGet]
        public async Task<double> Get()
        {
            return 3.14;
        }

    }
}
