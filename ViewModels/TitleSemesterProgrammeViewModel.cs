using Microsoft.AspNetCore.Mvc.Rendering;
using MVCFaculty.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCFaculty.ViewModels
{
    public class TitleSemesterProgrammeViewModel
    {

        public IList<Course> Courses { get; set; }
        public SelectList Semesters { get; set; }
        public SelectList Programmes { get; set; }
        public string CourseSemester { get; set; }
        public string CourseProgramme { get; set; }
        public string SearchString { get; set; }
    }
}
