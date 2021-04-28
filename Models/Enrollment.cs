using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCFaculty.Models
{
    public class Enrollment
    {
        public long EnrollmentID { get; set; }
        public int CourseID { get; set; }
        public long StudentID { get; set; }
        [StringLength(10)]
        public string Semester { get; set; }
        public int Year { get; set; }
        public Nullable<int> Grade { get; set; }
        [StringLength(255)]
        [Display(Name = "Seminal Url")]
        public string SeminalUrl { get; set; }
        [Display(Name = "Project Url")]
        [StringLength(255)]
        public string ProjectUrl { get; set; }
        [Display(Name = "Exam Points")]
        public Nullable<int> ExamPoints { get; set; }
        [Display(Name = "Seminal Points")]
        public Nullable<int> SeminalPoints { get; set; }
        [Display(Name = "Project Points")]

        public Nullable<int> ProjectPoints { get; set; }
        [Display(Name = "Additional Points")]

        public Nullable<int> AdditionalPoints { get; set; }
        [Display(Name = "Finish Date")]

        public Nullable<DateTime> FinishDate { get; set; }


        [ForeignKey("CourseID")]
        public Course Course { get; set; }

        [ForeignKey("StudentID")]
        public Student Student { get; set; }


    }
}