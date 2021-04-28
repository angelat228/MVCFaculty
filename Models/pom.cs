using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCFaculty.Models
{
    public class pom
    {
        public IEnumerable<Teacher> Teachers { get; set; }
        public IEnumerable<Course> Courses { get; set; }
        public IEnumerable<Enrollment> Enrollments { get; set; }
        public IEnumerable<Student> Students { get; set; }
    }
}
