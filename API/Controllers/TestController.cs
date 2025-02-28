using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private TestService _testService;

        public TestController(TestService testService) {
            _testService = testService;
        }
        [Authorize("Admin")]
        [HttpGet]
        public ActionResult getAllRoles()
        {
            var roles = _testService.GetAllRoles();
            return new JsonResult(roles);
        }
    }
}
