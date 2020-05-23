using cw2.Exceptions;
using cw2.Models;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace cw2.Services
{
    public class SqlServerDbService : IStudentsDbService
    {
        private const string ConString = "Data Source=db-mssql;Initial Catalog=s17524;Integrated Security=True";

        public IEnumerable<StudentInfoDto> GetStudents()
        {
            var list = new List<StudentInfoDto>();
            using (SqlConnection connection = new SqlConnection(ConString))
            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = "select s.FirstName, s.LastName, s.BirthDate, st.Name, e.Semester from Student s " +
                    "join Enrollment e on e.IdEnrollment = s.IdEnrollment join Studies st on st.IdStudy = e.IdStudy";
                connection.Open();

                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    var student = new StudentInfoDto
                    {
                        FirstName = dataReader["FirstName"].ToString(),
                        LastName = dataReader["LastName"].ToString(),
                        Name = dataReader["Name"].ToString(),
                        BirthDate = dataReader["BirthDate"].ToString(),
                        Semester = dataReader["Semester"].ToString()
                    };
                    list.Add(student);
                }
            }

            return list;
        }

        public StudentInfoDto GetStudent(string id)
        {
            var student = new StudentInfoDto();
            using (SqlConnection connection = new SqlConnection(ConString))
            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = "select s.FirstName, s.LastName, s.BirthDate, s.IndexNumber, st.Name, e.Semester " +
                    "from Student s " +
                    "join Enrollment e on e.IdEnrollment = s.IdEnrollment " +
                    "join Studies st on st.IdStudy = e.IdStudy " +
                    "where s.IndexNumber = @id";
                command.Parameters.AddWithValue("id", id);
                connection.Open();

                SqlDataReader dataReader = command.ExecuteReader();
                if (!dataReader.Read())
                {
                    return null;
                }

                student.FirstName = dataReader["FirstName"].ToString();
                student.LastName = dataReader["LastName"].ToString();
                student.Name = dataReader["Name"].ToString();
                student.BirthDate = dataReader["BirthDate"].ToString();
                student.Semester = dataReader["Semester"].ToString();
            }

            return student;
        }

        public IEnumerable<EnrollmentInfoDto> GetStudentEnrollments(string id)
        {
            var list = new List<EnrollmentInfoDto>();
            using (SqlConnection connection = new SqlConnection(ConString))
            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = "select s.IndexNumber, e.Semester, st.Name, e.StartDate " +
                    "from Student s " +
                    "join Enrollment e on e.IdEnrollment = s.IdEnrollment " +
                    "join Studies st on st.IdStudy = e.IdStudy " +
                    "where s.IndexNumber = @id";
                command.Parameters.AddWithValue("id", id);
                connection.Open();

                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    var enrollment = new EnrollmentInfoDto
                    {
                        Semester = dataReader["Semester"].ToString(),
                        Name = dataReader["Name"].ToString(),
                        StartDate = dataReader["StartDate"].ToString()
                    };
                    list.Add(enrollment);
                }
            }

            return list;
        }

        public EnrollmentDto EnrollStudent(EnrollStudentDto newStudent)
        {
            var enrollment = new EnrollmentDto();

            using (SqlConnection connection = new SqlConnection(ConString))
            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                connection.Open();
                var transaction = connection.BeginTransaction();
                command.Transaction = transaction;
                SqlDataReader dataReader = null;

                try
                {
                    command.CommandText = "select IdStudy " +
                        "from studies " +
                        "where name = @name ";
                    command.Parameters.AddWithValue("name", newStudent.Studies);

                    dataReader = command.ExecuteReader();
                    if (!dataReader.Read())
                    {
                        throw new BadRequestException("Study doesn't exist: " + newStudent.Studies);
                    }

                    if (GetStudent(newStudent.IndexNumber) != null)
                    {
                        throw new BadRequestException("Index number already taken: " + newStudent.IndexNumber);
                    }

                    var idStudy = dataReader["IdStudy"].ToString();
                    dataReader.Close();

                    command.CommandText = "select * " +
                        "from Enrollment " +
                        "where IdStudy=@idStudy " +
                        "   and Semester=1 ";
                    command.Parameters.AddWithValue("idStudy", idStudy);
                    dataReader = command.ExecuteReader();

                    string idEnrollment;
                    string startDate;

                    if (dataReader.Read())
                    {
                        idEnrollment = dataReader["IdEnrollment"].ToString();
                        startDate = dataReader["StartDate"].ToString();
                    }
                    else
                    {
                        dataReader.Close();
                        command.CommandText = "select max(IdEnrollment) as MaxIdEnrollment " +
                            "from Enrollment ";
                        dataReader.Read();
                        idEnrollment = (int.Parse(dataReader["currentMax"].ToString()) + 1).ToString();
                        startDate = "2020-03-29";
                        dataReader.Close();

                        command.CommandText = "insert into Enrollment(IdEnrollment, Semester, IdStudy, StartDate) " +
                            "values(@newId, @Semester, @IdStudy, @StartDate) ";
                        command.Parameters.AddWithValue("newId", idEnrollment);
                        command.Parameters.AddWithValue("IdStudy", idStudy);
                        command.Parameters.AddWithValue("Semester", 1);
                        command.Parameters.AddWithValue("StartDate", startDate);
                        command.ExecuteNonQuery();
                    }
                    dataReader.Close();

                    command.CommandText = "insert into Student (IndexNumber, FirstName, LastName, BirthDate, IdEnrollment) " +
                        "values(@IndexNumber, @FirstName, @LastName, @BirthDate, @IdEnrollment) ";
                    command.Parameters.AddWithValue("IndexNumber", newStudent.IndexNumber);
                    command.Parameters.AddWithValue("FirstName", newStudent.FirstName);
                    command.Parameters.AddWithValue("LastName", newStudent.LastName);
                    command.Parameters.AddWithValue("BirthDate", newStudent.BirthDate);
                    command.Parameters.AddWithValue("IdEnrollment", idEnrollment);
                    command.ExecuteNonQuery();

                    enrollment = new EnrollmentDto()
                    {
                        IdEnrollment = idEnrollment,
                        Semester = "1",
                        IdStudy = idStudy,
                        StartDate = startDate
                    };

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    if (dataReader != null)
                    {
                        dataReader.Close();
                        transaction.Rollback();
                    }
                    throw e;
                }
            }

            return enrollment;
        }

        public EnrollmentDto Promote(PromotionDto promotionDto)
        {
            var enrollment = new EnrollmentDto();

            using (SqlConnection connection = new SqlConnection(ConString))
            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                connection.Open();
                var transaction = connection.BeginTransaction();
                command.Transaction = transaction;
                SqlDataReader dataReader = null;

                try
                {
                    command.CommandText = "select IdStudy " +
                        "from studies " +
                        "where name = @studies ";
                    command.Parameters.AddWithValue("studies", promotionDto.Studies);

                    dataReader = command.ExecuteReader();
                    if (!dataReader.Read())
                    {
                        throw new NotFoundException("Study doesn't exist: " + promotionDto.Studies);
                    }
                    dataReader.Close();

                    command.CommandText = "select e.IdEnrollment, s.IdStudy, e.Semester, e.StartDate " +
                        "from enrollment e " +
                        "join studies s on e.IdStudy = s.IdStudy " +
                        "where s.name = @studies2 " +
                        " and e.semester = @semester2";
                    command.Parameters.AddWithValue("studies2", promotionDto.Studies);
                    command.Parameters.AddWithValue("semester2", promotionDto.Semester);

                    dataReader = command.ExecuteReader();
                    if (!dataReader.Read())
                    {
                        throw new NotFoundException("Semester for selected studies doesn't exist: semester-" + promotionDto.Semester + ", studies-" + promotionDto.Studies);
                    }
                    else
                    {
                        dataReader.Close();
                        command.CommandText = "exec promotions @Studies3, @Semester3";
                        command.Parameters.AddWithValue("Studies3", promotionDto.Studies);
                        command.Parameters.AddWithValue("Semester3", promotionDto.Semester);
                        dataReader = command.ExecuteReader();

                        if (dataReader.Read())
                        {
                            enrollment = new EnrollmentDto()
                            {
                                IdEnrollment = dataReader["IdEnrollment"].ToString(),
                                Semester = dataReader["Semester"].ToString(),
                                IdStudy = dataReader["IdStudy"].ToString(),
                                StartDate = dataReader["StartDate"].ToString()
                            };
                        }

                        dataReader.Close();
                        transaction.Commit();
                    }
                }
                catch (Exception e)
                {
                    if (dataReader != null)
                    {
                        dataReader.Close();
                        transaction.Rollback();
                    }
                    throw e;
                }
            }

            return enrollment;
        }
    }
}
