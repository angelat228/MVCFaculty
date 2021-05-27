
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using MVCFaculty.Data;
using MVCFaculty.Models;
using MVCFaculty.ViewModels;
using Microsoft.AspNetCore.Http;
using University.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace MVCFaculty.Controllers
{
    public class EnrollmentsController : Controller
    {
        private readonly MVCFacultyContext _context;
        private readonly IWebHostEnvironment WebHostEnvironment;


        public EnrollmentsController(MVCFacultyContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            WebHostEnvironment = webHostEnvironment;
        }
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetStudentsByCourse(int id, int? yearInt = 0)
        {
            var course = _context.Courses.Where(l => l.CourseID == id).FirstOrDefault();
            ViewData["courseName"] = course.Title;
            TempData["selectedCourse"] = id.ToString();
            var enrollments = _context.Enrollments.Where(s => s.CourseID == id);
            string tch = TempData["selectedTeacher"].ToString();
            TempData.Keep();
            ViewData["tch"] = tch;
            enrollments = enrollments.Include(c => c.Student);
            if (yearInt == 0)
            {
                string yearStr = DateTime.Now.Year.ToString();
                yearInt = Int32.Parse(yearStr);
            }
            enrollments = enrollments.Where(s => s.Year == yearInt);
            ViewData["currentYear"] = yearInt;
            return View(enrollments);
        }
        [Authorize(Roles = "Teacher")]

        public async Task<IActionResult> EditByProfessor(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            string s = null;
            if (TempData["selectedCourse"] != null)
                s = TempData["selectedCourse"].ToString();
            TempData.Keep();
            ViewData["selectedCrs"] = s;
            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(m => m.EnrollmentID == id);
            if (enrollment == null)
            {
                return NotFound();
            }
            return View(enrollment);
        }

        [HttpPost, ActionName("editByProfessor")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> EditByProfessorPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            int crsId = 1;
            string crs = null;
            if (TempData["selectedCourse"] != null)
            {
                crs = TempData["selectedCourse"].ToString();
                crsId = Int32.Parse(crs);
            }

            var enrollmentToUpdate = await _context.Enrollments.FirstOrDefaultAsync(s => s.EnrollmentID == id);
            await TryUpdateModelAsync<Enrollment>(
                 enrollmentToUpdate,
                 "", s => s.ExamPoints, s => s.SeminalPoints, s => s.ProjectPoints, s => s.AdditionalPoints,
                 s => s.Grade, s => s.FinishDate);

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction("getStudentsByCourse", "Enrollments", new { id = crsId });
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex vari  able name and write a log.)
                ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists, " +
                    "see your system administrator.");
            }
            return View(enrollmentToUpdate);
        }

        private string UploadedFile(IFormFile file)
        {
            string uniqueFileName = null;
            if (file != null)
            {
                string uploadsFolder = Path.Combine(WebHostEnvironment.WebRootPath, "seminals");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> EditByStudent(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            string c = null;
            if (TempData["selectedStudent"] != null)
                c = TempData["selectedStudent"].ToString();
            TempData.Keep();
            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(m => m.EnrollmentID == id);
            if (enrollment == null)
            {
                return NotFound();
            }
            return View(enrollment);
        }

        [HttpPost, ActionName("editByStudent")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> EditByStudentPost(int? id, IFormFile semUrl)
        {
            if (id == null)
            {
                return NotFound();
            }

            int stId = 1;
            string st = null;
            if (TempData["selectedStudent"] != null)
            {
                st = TempData["selectedStudent"].ToString();
                stId = Int32.Parse(st);
            }

            var enrollmentToUpdate = await _context.Enrollments.FirstOrDefaultAsync(s => s.EnrollmentID == id);
            enrollmentToUpdate.SeminalUrl = UploadedFile(semUrl);
            await TryUpdateModelAsync<Enrollment>(
                enrollmentToUpdate,
                "", s => s.ProjectUrl);

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction("getCoursesByStudent", "Enrollments", new { id = stId });
            }
            catch (DbUpdateException /* ex */)
            {
                ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists, " +
                    "see your system administrator.");
            }
            //}
            return View(enrollmentToUpdate);
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetCoursesByStudent(int id)
        {
            TempData["selectedStudent"] = id.ToString();
            var student = _context.Students.Where(s => s.ID == id).First();
            ViewData["studentName"] = student.FullName;
            var enrollments = _context.Enrollments.Where(s => s.StudentID == id);
            enrollments = enrollments.Include(c => c.Course);
            return View(enrollments);
        }
        // GET: Enrollments
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var rSWEBproektContext = _context.Enrollments.Include(e => e.Course).Include(e => e.Student);
            return View(await rSWEBproektContext.ToListAsync());
        }

        // GET: Enrollments/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            string s = null;
            if (TempData["selectedCourse"] != null)
                s = TempData["selectedCourse"].ToString();
            TempData.Keep();
            ViewData["selectedCrs"] = s;
            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(m => m.EnrollmentID == id);
            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }
        /*
        public async Task<IActionResult> EnrollStudents(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = _context.Courses.Where(m => m.CourseID == id)
               .Include(m => m.Enrollments).First();
            if (course == null)
            {
                return NotFound();
            }
            var EnrollmentEditVM = new EnrollmentEditViewModel
            {
                Course = course,
                SelectedStudents = course.Enrollments.Select(m => m.StudentID),
                StudentList = new MultiSelectList(_context.Students, "Id", "FullName")
            };
            return View(EnrollmentEditVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollStudents(int id, int year, string semester, EnrollmentEditViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    IEnumerable<long> listStudents = viewModel.SelectedStudents;
                    IQueryable<Enrollment> toBeRemoved = _context.Enrollments.Where(s => !listStudents.Contains(s.StudentID) && s.CourseID == id);
                    _context.Enrollments.RemoveRange(toBeRemoved);
                    IEnumerable<long> existStudents = _context.Enrollments.Where(s => listStudents.Contains(s.StudentID) && s.CourseID == id).Select(s => s.StudentID);
                    IEnumerable<long> newStudents = listStudents.Where(s => !existStudents.Contains(s));

                    foreach (int studentId in newStudents)
                        _context.Enrollments.Add(new Enrollment
                        {
                            StudentID = studentId,
                            CourseID = id,
                            Semester = semester,
                            Year = year
                        });

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(CoursesController.Index));
            }
            return View(viewModel);
        }*/
        // GET: Enrollments/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["CourseID"] = new SelectList(_context.Courses, "CourseID", "Title");
            ViewData["StudentID"] = new SelectList(_context.Students, "Id", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("EnrollmentID,CourseID,StudentID,Semestar,Grade,Year,SeminalUrl,ProjectUrl,ExamPoints,SeminalPoints,ProjectPoints,AdditionalPoints,FinishDate")] Enrollment enrollment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(enrollment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseID"] = new SelectList(_context.Courses, "CourseID", "Title", enrollment.CourseID);
            ViewData["StudentID"] = new SelectList(_context.Students, "Id", "FullName", enrollment.StudentID);
            return View(enrollment);
        }

        // GET: Enrollments/Edit/5
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(m => m.EnrollmentID == id);
            if (enrollment == null)
            {
                return NotFound();
            }
            ViewData["CourseID"] = new SelectList(_context.Courses, "CourseID", "Title", enrollment.CourseID);
            ViewData["StudentID"] = new SelectList(_context.Students, "Id", "FullName", enrollment.StudentID);
            return View(enrollment);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var enrollmentToUpdate = await _context.Enrollments.FirstOrDefaultAsync(s => s.EnrollmentID == id);
            await TryUpdateModelAsync<Enrollment>(
                enrollmentToUpdate,
                "",
                s => s.FinishDate);
            //{
            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.)
                ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists, " +
                    "see your system administrator.");
            }
            //}
            return View(enrollmentToUpdate);
        }


        // GET: Enrollments/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(m => m.EnrollmentID == id);
            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }

        // POST: Enrollments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EnrollmentExists(int id)
        {
            return _context.Enrollments.Any(e => e.EnrollmentID == id);
        }

        //GET: Enrollments/CreateEnrollment
        [Authorize(Roles = "Admin")]
        public IActionResult CreateEnrollment()
        {
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title");
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FullName");
            return View();
        }

        //POST: Enrollments/CreateEnrollment
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEnrollment([Bind("Id,CourseId,StudentId, Semester,Year")] Enrollment enrollment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(enrollment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(EnrollmentList));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", enrollment.CourseID);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FullName", enrollment.StudentID);
            return View(enrollment);
        }

        // GET: Enrollments/FinishEnrollment/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> FinishEnrollment(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", enrollment.CourseID);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FullName", enrollment.StudentID);
            return View(enrollment);
        }

        // POST: Enrollments/FinishEnrollment/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinishEnrollment(int id, [Bind("Id,CourseId,StudentId,Semester,Year,Grade,SeminalUrl,ProjectUrl,ExamPoints,SeminalPoints,ProjectPoints,AdditionalPoints,FinishDate")] Enrollment enrollment)
        {
            if (id != enrollment.EnrollmentID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrollmentExists(enrollment.EnrollmentID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(EnrollmentList));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Title", enrollment.CourseID);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FullName", enrollment.StudentID);
            return View(enrollment);
        }

        private bool EnrollmentExists(long enrollmentID)
        {
            throw new NotImplementedException();
        }

        public async Task<IActionResult> EnrollmentList()
        {
            var universityContext = _context.Enrollments.Include(e => e.Course).Include(e => e.Student);
            return View(await universityContext.ToListAsync());
        }

        public IActionResult EnrollStudents()
        {
            IEnumerable<Student> students = _context.Students.AsEnumerable();
            students = students.OrderBy(s => s.FullName);
            IEnumerable<Course> courses = _context.Courses.AsEnumerable();
            courses = courses.OrderBy(s => s.Title);

            StudentsCoursesEnrollViewModel viewModel = new StudentsCoursesEnrollViewModel
            {
                StudentList = new MultiSelectList(students.AsEnumerable(), "Id", "FullName"),
                CourseList = new SelectList(courses.AsEnumerable(), "Id", "Title")
            };
            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollStudents(StudentsCoursesEnrollViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                Course course = viewModel.Course;
                var courseId = course.CourseID;
                IEnumerable<SelectListItem> listCourses = viewModel.CourseList;
                IEnumerable<int> listStudents = viewModel.SelectedStudents;
                foreach (int studentId in listStudents)
                    _context.Enrollments.Add(new Enrollment { StudentID= studentId, CourseID = courseId });
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }



    }
}
