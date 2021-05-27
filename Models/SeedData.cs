using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MVCFaculty.Areas.Identity.Data;
using MVCFaculty.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCFaculty.Models
{
    public class SeedData
    {
        public static async Task CreateUserRoles(IServiceProvider serviceProvider)
        {
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<MVCFacultyUSER>>();
            IdentityResult roleResult;

            //Add Admin Role
            var roleCheck = await RoleManager.RoleExistsAsync("Admin");
            if (!roleCheck) { roleResult = await RoleManager.CreateAsync(new IdentityRole("Admin")); }

            //Add Admin User
            MVCFacultyUSER user = await UserManager.FindByEmailAsync("admin@mvcfaculty.com");
            if (user == null)
            {
                var User = new MVCFacultyUSER();
                User.Email = "admin@mvcfaculty.com";
                User.UserName = "admin@mvcfaculty.com";
                string userPWD = "Admin123";
                IdentityResult chkUser = await UserManager.CreateAsync(User, userPWD);
                //Add default User to Role Admin
                if (chkUser.Succeeded) { var result1 = await UserManager.AddToRoleAsync(User, "Admin"); }
            }

            //Add Teacher Role
            roleCheck = await RoleManager.RoleExistsAsync("Teacher");
            if (!roleCheck)
            {
                roleResult = await RoleManager.CreateAsync(new IdentityRole("Teacher"));
            }

            user = await UserManager.FindByEmailAsync("edna@mvcfaculty.com");
            if (user == null)
            {
                var User = new MVCFacultyUSER();
                User.Email = "edna@mvcfaculty.com";
                User.UserName = "edna@mvcfaculty.com";
                User.TeacherId = 1;
                string userPWD = "Edna123";
                IdentityResult chkUser = await UserManager.CreateAsync(User, userPWD);
                if (chkUser.Succeeded) { var result1 = await UserManager.AddToRoleAsync(User, "Teacher"); }
            }

            //Add Student Role
            roleCheck = await RoleManager.RoleExistsAsync("Student");
            if (!roleCheck)
            {
                roleResult = await RoleManager.CreateAsync(new IdentityRole("Student"));
            }
        }


        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new MVCFacultyContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<MVCFacultyContext>>()))
            {
                CreateUserRoles(serviceProvider).Wait();

                if (context.Courses.Any() || context.Teachers.Any() || context.Students.Any())
                {
                    return;
                }


                var students = new Student[]
            {
            new Student{StudentId="100",FirstName="Bart",LastName="Simpson", EnrollmentDate=DateTime.Parse("2000-09-01"),
             AcquiredCredits=100, CurrentSemestar=6,EducationLevel="Primary education", ProfilePicture="https://upload.wikimedia.org/wikipedia/en/a/aa/Bart_Simpson_200px.png" },
            new Student{StudentId="101",FirstName="Ralph",LastName="Wiggum", EnrollmentDate=DateTime.Parse("2000-09-01"),
             AcquiredCredits=100, CurrentSemestar=6,EducationLevel="Primary education", ProfilePicture="https://static.wikia.nocookie.net/simpsons/images/b/ba/Ralph_ballet_tapped_out.png/revision/latest?cb=20150808022743"},
            new Student{StudentId="102",FirstName="Nelson",LastName="Muntz", EnrollmentDate=DateTime.Parse("2000-09-01"),
             AcquiredCredits=100, CurrentSemestar=6,EducationLevel="Primary education", ProfilePicture="https://upload.wikimedia.org/wikipedia/en/c/c6/Nelson_Muntz.PNG"},
            new Student{StudentId="103",FirstName="Jimbo",LastName="Jones", EnrollmentDate=DateTime.Parse("2000-09-01"),
             AcquiredCredits=100, CurrentSemestar=6,EducationLevel="Primary education", ProfilePicture="http://4.bp.blogspot.com/-OhfCOqGz3pk/Tq2y4xCtwcI/AAAAAAAAAhY/bv8gpkJuJWM/s1600/Jimbo%252520%2528the%252520bully%2529%252520excited.png"},
            new Student{StudentId="104",FirstName="Martin",LastName="Prince", EnrollmentDate=DateTime.Parse("2000-09-01"),
             AcquiredCredits=100, CurrentSemestar=6,EducationLevel="Primary education", ProfilePicture="https://static.wikia.nocookie.net/simpsons/images/4/46/Martin_Prince.png/revision/latest?cb=20201223183916"},
            };

                foreach (Student s in students)
                {
                    context.Students.Add(s);
                }
                context.SaveChanges();

                // if(context.Teachers.Any()){
                //  return;
                // }
                var teachers = new Teacher[]
                {
            new Teacher{ FirstName="Edna", LastName="Krabappel", Degree="Bachelor’s Degree", AcademicRank="Associate  professor",
             OfficeNumber="111",HireDate=DateTime.Parse("1988-06-03"), ProfilePicture="https://i.pinimg.com/originals/69/79/88/6979885b413c81abb2cec2b24d9a46fb.jpg"},
            new Teacher{ FirstName="Ned", LastName="Flanders", Degree="Doctorate Degree", AcademicRank=" Associate Professor",
             OfficeNumber="505",HireDate=DateTime.Parse("1994-11-02"), ProfilePicture="https://www.pngkit.com/png/full/41-416703_the-simpsons-ned-flanders-ned-flanders.png"},
            new Teacher{ FirstName="Elizabeth", LastName="Hoover", Degree="Bachelor’s Degree", AcademicRank="Professor",
             OfficeNumber="223",HireDate=DateTime.Parse("1989-05-22"), ProfilePicture="https://i.pinimg.com/originals/b7/4f/ae/b74faea8de35d22b703b6ae32f891a92.png"},
            new Teacher{ FirstName="Dewey", LastName="Largo", Degree="Doctorate Degree", AcademicRank="Professor",
             OfficeNumber="305",HireDate=DateTime.Parse("1992-02-27"), ProfilePicture="https://i.pinimg.com/originals/53/e5/15/53e51516a408e65f9226d4732599e759.png"},

                };
                foreach (Teacher t in teachers)
                {
                    context.Teachers.Add(t);
                }
                context.SaveChanges();

                // if(context.Courses.Any()){
                //   return;
                //}

                var courses = new Course[]
                {
            new Course{CourseID=1050,Title="Mathematics",Credits=4,Semester=6,Programme="Programme 1", EducationLevel="Primary education",
            FirstTeacherId=teachers.Single(s=>s.LastName=="Krabappel").TeacherId, SecondTeacherId=teachers.Single(s=>s.LastName=="Hoover").TeacherId },
            new Course{CourseID=4022,Title="Science",Credits=4,Semester=6,Programme="Programme 1", EducationLevel="Primary education",
            FirstTeacherId=teachers.Single(s=>s.LastName=="Krabappel").TeacherId, SecondTeacherId=teachers.Single(s=>s.LastName=="Largo").TeacherId},
            new Course{CourseID=4041,Title="Art",Credits=4,Semester=6,Programme="Programme 1", EducationLevel="Primary education",
            FirstTeacherId=teachers.Single(s=>s.LastName=="Flanders").TeacherId, SecondTeacherId=teachers.Single(s=>s.LastName=="Largo").TeacherId},
            new Course{CourseID=1045,Title="Physics",Credits=4,Semester=6,Programme="Programme 1", EducationLevel="Primary education",
            FirstTeacherId=teachers.Single(s=>s.LastName=="Flanders").TeacherId, SecondTeacherId=teachers.Single(s=>s.LastName=="Hoover").TeacherId},

                };
                foreach (Course c in courses)
                {
                    context.Courses.Add(c);
                }
                context.SaveChanges();



                context.Enrollments.AddRange(

            new Enrollment { StudentID = students.Single(s => s.LastName == "Simpson").ID, CourseID = courses.Single(c => c.Title == "Mathematics").CourseID, Semester = "6", Year = 2,
                Grade = 5, SeminalUrl = "fff", ProjectUrl = "ab1", ExamPoints = 100, SeminalPoints = 100, ProjectPoints = 100, AdditionalPoints = 5, FinishDate = DateTime.Parse("2002-10-01") },
            new Enrollment { StudentID = students.Single(s => s.LastName == "Simpson").ID, CourseID = courses.Single(c => c.Title == "Mathematics").CourseID, Semester = "6", Year = 2,
                Grade = 2, SeminalUrl = "gff", ProjectUrl = "ab2", ExamPoints = 80, SeminalPoints = 26, ProjectPoints = 100, AdditionalPoints = 0, FinishDate = DateTime.Parse("2002-10-01") },
            new Enrollment { StudentID = students.Single(s => s.LastName == "Wiggum ").ID, CourseID = courses.Single(c => c.Title == "Science").CourseID, Semester = "6", Year = 2,
                Grade = 2, SeminalUrl = "wws", ProjectUrl = "ab3", ExamPoints = 50, SeminalPoints = 24, ProjectPoints = 100, AdditionalPoints = 3, FinishDate = DateTime.Parse("2002-10-01") },
            new Enrollment { StudentID = students.Single(s => s.LastName == "Wiggum ").ID, CourseID = courses.Single(c => c.Title == "Science").CourseID, Semester = "6", Year = 2,
                Grade = 3, SeminalUrl = "yyy", ProjectUrl = "ab4", ExamPoints = 70, SeminalPoints = 50, ProjectPoints = 50, AdditionalPoints = 2, FinishDate = DateTime.Parse("2002-10-01") },
            new Enrollment { StudentID = students.Single(s => s.FirstName == "Nelson").ID, CourseID = courses.Single(c => c.Title == "Art").CourseID, Semester = "6", Year = 2,
                Grade = 5, SeminalUrl = "hhg", ProjectUrl = "ab5", ExamPoints = 50, SeminalPoints = 95, ProjectPoints = 50, AdditionalPoints = 5, FinishDate = DateTime.Parse("2002-10-01") },
            new Enrollment { StudentID = students.Single(s => s.FirstName == "Jimbo").ID, CourseID = courses.Single(c => c.Title == "Art").CourseID, Semester = "6", Year = 2,
                Grade = 4, SeminalUrl = "qqw", ProjectUrl = "ab6", ExamPoints = 40, SeminalPoints = 80, ProjectPoints = 60, AdditionalPoints = 3, FinishDate = DateTime.Parse("2000-10-01") },
            new Enrollment { StudentID = students.Single(s => s.FirstName == "Martin").ID, CourseID = courses.Single(c => c.Title == "Physics").CourseID, Semester = "6", Year = 2,
                Grade = 4, SeminalUrl = "lol", ProjectUrl = "ab7", ExamPoints = 70, SeminalPoints = 70, ProjectPoints = 60, AdditionalPoints = 6, FinishDate = DateTime.Parse("2002-10-01") }
            );
                /* foreach (Enrollment e in enrollments)
                 {
                     context.Enrollments.Add(e);
                 }*/
                context.SaveChanges();
            }
        }
    }
}
