using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using cw2.DAL;
using cw2.Models;
using cw2.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cw2.Controllers
{

    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private const string S1 = "Kowalski";
        private const string S2 = "Majewski";
        private const string S3 = "Andrzejewski";
        private readonly IStudentsDbService _dbService;

        public StudentsController(IStudentsDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet]
        public IActionResult GetStudents(string orderBy)
        {
            try
            {
                return Ok(_dbService.GetStudents());
            }
            catch (Exception e)
            {
                return BadRequest("Exception: " + e.Message + "\n" + e.StackTrace);
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetStudent(string id)
        {
            try
            {
                return Ok(_dbService.GetStudent(id));
            }
            catch (Exception e)
            {
                return BadRequest("Exception: " + e.Message + "\n" + e.StackTrace);
            }
        }

        [HttpGet("enrollments/{id}")]
        public IActionResult GetStudentEnrollments(string id)
        {
            try
            {
                return Ok(_dbService.GetStudentEnrollments(id));
            }
            catch (Exception e)
            {
                return BadRequest("Exception: " + e.Message + "\n" + e.StackTrace);
            }
        }

        [HttpPost]
        public IActionResult CreateStudent(StudentDto student)
        {
            student.IndexNumber = $"s{new Random().Next(1, 20000)}";
            return Ok(student);
        }

        [HttpPut]
        public IActionResult UpdateStudent(StudentDto student)
        {
            return Ok("Aktualizacja dokończona");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteStudent(int id)
        {
            return Ok("Usuwanie ukończone");
        }
    }
}