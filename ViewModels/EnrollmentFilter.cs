using MVCFaculty.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCFaculty.ViewModels
{
    public class EnrollmentFilter
    {

        public IList<Enrollment> Enrollments { get; set; }

        public int EnrollmentYear { get; set; }
    }
}
