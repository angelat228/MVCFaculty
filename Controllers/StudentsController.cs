using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVCFaculty.Data;
using MVCFaculty.Models;
using MVCFaculty.ViewModels;

namespace MVCFaculty.Controllers
{
    public class StudentsController : Controller
    {
        private readonly MVCFacultyContext _context;
        private  IWebHostEnvironment _webHostEnvironment { get; }

        public StudentsController(MVCFacultyContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Students
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;
            var students = from s in _context.Students
                           select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                students = students.Where(s => s.LastName.Contains(searchString)||s.FirstName.Contains(searchString)||s.StudentId.Contains(searchString));
            }
            return View(await students.AsNoTracking().ToListAsync());
        }

        // GET: Students/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Enrollments)
            .ThenInclude(e => e.Course)
        .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Students/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()

        {
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(StudentForm Vmodel)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = UploadedFile(Vmodel);

                Student student = new Student
                {
                    ProfilePicture = uniqueFileName,
                    StudentId = Vmodel.Index,
                    FirstName = Vmodel.FirstName,
                    LastName = Vmodel.LastName,
                    EnrollmentDate = Vmodel.EnrollmentDate,
                    AcquiredCredits = Vmodel.AcquiredCredits,
                    CurrentSemestar = Vmodel.CurrentSemestar,
                    EducationLevel = Vmodel.EducationLevel,
                    Enrollments = Vmodel.Courses,
                };

                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        private string UploadedFile(StudentForm model)
        {
            string uniqueFileName = null;

            if (model.ProfilePicture != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ProfilePicture.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.ProfilePicture.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();

            }


            var student = await _context.Students.FindAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            StudentForm Vmodel = new StudentForm
            {
                Id = student.ID,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Index = student.StudentId,
                EnrollmentDate = student.EnrollmentDate,
                AcquiredCredits = student.AcquiredCredits,
                CurrentSemestar = student.CurrentSemestar,
                EducationLevel = student.EducationLevel,
                Courses = student.Enrollments
            };
            ViewData["StudentFullName"] = _context.Students.Where(t => t.ID == id).Select(t => t.FullName).FirstOrDefault();


            return View(Vmodel);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(long id, StudentForm Vmodel)
        {

            if (id != Vmodel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string uniqueFileName = UploadedFile(Vmodel);
                    Student student = new Student
                    {
                        ID = Vmodel.Id,
                        FirstName = Vmodel.FirstName,
                        LastName = Vmodel.LastName,
                        ProfilePicture = uniqueFileName,
                        EnrollmentDate = Vmodel.EnrollmentDate,
                        CurrentSemestar = Vmodel.CurrentSemestar,
                        AcquiredCredits = Vmodel.AcquiredCredits,
                        StudentId = Vmodel.Index,
                        EducationLevel = Vmodel.EducationLevel,
                        Enrollments = Vmodel.Courses
                    };
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(Vmodel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(Vmodel);
        }

        // GET: Students/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(long? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (student == null)
            {
                return NotFound();
            }

            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["ErrorMessage"] =
                    "Delete failed. Try again, and if the problem persists " +
                    "see your system administrator.";
            }

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var student = await _context.Students.FindAsync(id);
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(long id)
        {
            return _context.Students.Any(e => e.ID == id);
        }


      /*  public async Task<IActionResult> StudentsByCourse(long? id)
        {
            IQueryable<Course> courses = _context.Courses.Include(c => c.FirstTeacher).Include(c => c.SecondTeacher).AsQueryable();

            IQueryable<Enrollment> enrollments = _context.Enrollments.AsQueryable();
            enrollments = enrollments.Where(s => s.StudentID == id); 
            IEnumerable<int> enrollmentsIDS = enrollments.Select(e => e.CourseID).Distinct(); 

            courses = courses.Where(s => enrollmentsIDS.Contains(s.CourseID));  

            courses = courses.Include(c => c.Enrollments).ThenInclude(c => c.Student);

            ViewData["StudentName"] = _context.Students.Where(t => t.ID == id).Select(t => t.FullName).FirstOrDefault();
            ViewData["studentId"] = id;
            return View(courses);
        }*/

    }
}
