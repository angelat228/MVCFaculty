using Microsoft.AspNetCore.Mvc.Rendering;
using MVCFaculty.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCFaculty.ViewModels
{
    public class ViewModel
    {
        public Course course { get; set; }
        public IEnumerable<long> selectedStudents { get; set; }
        public IEnumerable<SelectListItem> studentList { get; set; }
    }
}
