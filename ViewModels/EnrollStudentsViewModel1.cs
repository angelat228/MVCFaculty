using Microsoft.AspNetCore.Mvc.Rendering;
using MVCFaculty.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MVCFaculty.ViewModels
{
    public class EnrollStudentsViewModel1
    {
        public Enrollment Enrollments { get; set; }
        public int Year { get; set; }
        public string Semester { get; set; }
        public virtual Course Course { get; set; }
        public int CourseId { get; set; }
        [Display(Name = "Selected Students")]
        public IEnumerable<int> SelectedStudents { get; set; }
        public SelectList Courses { get; set; }
        public SelectList StudentsList { get; set; }
    }
}
