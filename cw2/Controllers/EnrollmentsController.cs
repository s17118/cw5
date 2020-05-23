using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using cw2.Exceptions;
using cw2.Models;
using cw2.Services;
using Microsoft.AspNetCore.Mvc;

namespace cw2.Controllers
{

    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentsController : Controller
    {
        private readonly IStudentsDbService _dbService;

        public EnrollmentsController(IStudentsDbService dbService)
        {
            _dbService = dbService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult EnrollStudent(EnrollStudentDto newStudent)
        {
            try
            {
                return CreatedAtAction("EnrollStudent", _dbService.EnrollStudent(newStudent));
            }
            catch (Exception e)
            {
                return BadRequest("Exception: " + e.Message + "\n" + e.StackTrace);
            }
        }

        [HttpPost("promotions")]
        public IActionResult Promote(PromotionDto promotionDto)
        {
            try
            {
                return CreatedAtAction("Promote", _dbService.Promote(promotionDto));
            }
            catch (NotFoundException e)
            {
                return NotFound("NotFoundException: " + e.Message + "\n" + e.StackTrace);
            }
            catch (Exception e)
            {
                return BadRequest("Exception: " + e.Message + "\n" + e.StackTrace);
            }
        }
    }
}