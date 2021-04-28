using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MVCFaculty.ViewModels
{
    public class EnrollmentView
    {
        public long Id { get; set; }
        public int CourseId { get; set; }
        public long StudentId { get; set; }

        [StringLength(10)]
        public string Semester { get; set; }

        public int Year { get; set; }

        public int? Grade { get; set; }

        public string SeminalUrl { get; set; }

        [StringLength(255)]
        public string ProjectUrl { get; set; }

        public int? ExamPoints { get; set; }
        public int? SeminalPoints { get; set; }
        public int? ProjectPoints { get; set; }
        public int? AdditionalPoints { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FinishDate { get; set; }
    }
}
