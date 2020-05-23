using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cw2.Models;
using System.Linq.Expressions;

namespace cw2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentsEfController : ControllerBase
    {
        private readonly s17118Context _context;

        public EnrollmentsEfController(s17118Context context)
        {
            _context = context;
        }

        // GET: api/EnrollmentsEf
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Enrollment>>> GetEnrollment()
        {
            return await _context.Enrollment.ToListAsync();
        }

        // GET: api/EnrollmentsEf/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Enrollment>> GetEnrollment(int id)
        {
            var enrollment = await _context.Enrollment.FindAsync(id);

            if (enrollment == null)
            {
                return NotFound();
            }

            return enrollment;
        }

        // PUT: api/EnrollmentsEf/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEnrollment(int id, Enrollment enrollment)
        {
            if (id != enrollment.IdEnrollment)
            {
                return BadRequest();
            }

            _context.Entry(enrollment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EnrollmentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/EnrollmentsEf
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Enrollment>> PostEnrollment(Enrollment enrollment)
        {
            _context.Enrollment.Add(enrollment);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (EnrollmentExists(enrollment.IdEnrollment))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetEnrollment", new { id = enrollment.IdEnrollment }, enrollment);
        }

        // DELETE: api/EnrollmentsEf/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Enrollment>> DeleteEnrollment(int id)
        {
            var enrollment = await _context.Enrollment.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }

            _context.Enrollment.Remove(enrollment);
            await _context.SaveChangesAsync();

            return enrollment;
        }

        // POST: api/EnrollmentsEf/enroll/Informatyka
        [HttpPost("enroll/{studies}")]
        public async Task<ActionResult<Enrollment>> PostStudentEnrollment(string studies, Student student)
        {
            if (!StudiesExists(studies))
            {
                return BadRequest("Study doesn't exist: " + studies);
            }

            Enrollment enrollment = new Enrollment
            {
                IdEnrollment = _context.Enrollment.Max(e => e.IdEnrollment) + 1,
                Semester = 1,
                IdStudy = _context.Studies.Where(s => s.Name == studies).First().IdStudy,
                StartDate = Convert.ToDateTime("2020-03-29")
            };

            student.IdEnrollment = enrollment.IdEnrollment;

            _context.Enrollment.Add(enrollment);
            _context.Student.Add(student);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (StudentExists(student.IndexNumber))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("PostStudentEnrollment", new { id = enrollment.IdEnrollment }, enrollment);
        }

        // POST: api/EnrollmentsEf/enroll/Informatyka
        [HttpPost("promotions/{studies}/{semester}")]
        public async Task<ActionResult<Enrollment>> PostStudentEnrollment(string studies, int semester)
        {
            if (!StudiesExists(studies))
            {
                return BadRequest("Study doesn't exist: " + studies);
            }

            int idStudy = _context.Studies.Where(s => s.Name == studies).First().IdStudy;
            if (!_context.Enrollment.Any(enrollmentExist(semester, idStudy)))
            {
                return NotFound("Semester for selected studies doesn't exist: semester-" + semester + ", studies-" + studies);
            }

            _context.Database.ExecuteSqlCommand("exec promotions " + studies + ", " + semester);

            int idEnrollment = _context.Enrollment.Where(enrollmentExist(semester + 1, idStudy)).First().IdEnrollment;
            var enrollment = _context.Enrollment.Where(e => e.IdEnrollment == idEnrollment).First();

            return CreatedAtAction("PostStudentEnrollment", new { id = enrollment.IdEnrollment }, enrollment);
        }

        private static Expression<Func<Enrollment, bool>> enrollmentExist(int semester, int idStudy)
        {
            return e => e.IdStudy == idStudy && e.Semester == semester;
        }

        private bool EnrollmentExists(int id)
        {
            return _context.Enrollment.Any(e => e.IdEnrollment == id);
        }

        private bool StudentExists(string id)
        {
            return _context.Student.Any(e => e.IndexNumber == id);
        }

        private bool StudiesExists(string studyName)
        {
            return _context.Studies.Any(e => e.Name == studyName);
        }
    }
}
