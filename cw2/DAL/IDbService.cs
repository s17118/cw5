using cw2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cw2.DAL
{
    public interface IDbService
    {
        public IEnumerable<StudentDto> GetStudents();
    }
}
