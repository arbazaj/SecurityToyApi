using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SecurityToy.Models;

namespace SecurityToy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        [HttpGet]
        //[Authorize(Roles = Role.Manager+","+Role.Admin)]
        [Authorize(Roles = Role.Admin)]
        public ActionResult<IEnumerable<User>> Get()
        {
            try
            {
                return Ok(new { status = 200, title = "Request successful.", data = new { roles = Role.GetRoleList() } });
            }
            catch (Exception)
            {
                return BadRequest(new { status = 400, title = "Something went wrong" });
            }
        }
    }
}