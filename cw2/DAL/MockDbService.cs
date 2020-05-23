using cw2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cw2.DAL
{
    public class MockDbService : IDbService
    {
        private static IEnumerable<StudentDto> _students;

        static MockDbService()
        {
            _students = new List<StudentDto>
            {
                new StudentDto{IdStudent=1, FirstName="Jan", LastName="Kowalski"},
                new StudentDto{IdStudent=2, FirstName="Anna", LastName="Malewski"},
                new StudentDto{IdStudent=3, FirstName="Andrzej", LastName="Andrzejewicz"}
            };
        }

        public IEnumerable<StudentDto> GetStudents()
        {
            return _students;
        }
    }
}
