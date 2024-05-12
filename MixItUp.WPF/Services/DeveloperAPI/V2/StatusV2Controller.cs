using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace MixItUp.WPF.Services.DeveloperAPI.V2
{
    //Prefix
    [Route("api/v2/status")]
    public class StatusV2Controller : ControllerBase
    {
        [Route("version")]
        [HttpGet]
        public IActionResult GetVersion()
        {
            return Ok(Assembly.GetEntryAssembly().GetName().Version.ToString());
        }
    }
}