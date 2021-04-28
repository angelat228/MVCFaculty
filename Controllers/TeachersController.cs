using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVCFaculty.Data;
using MVCFaculty.Models;
using MVCFaculty.ViewModels;

namespace MVCFaculty.Controllers
{
    public class TeachersController : Controller
    {
        private readonly MVCFacultyContext _context;

        public TeachersController(MVCFacultyContext context)
        {
            _context = context;
        }

        // GET: Teachers
        public async Task<IActionResult> Index(int? id, int? courseID, string search)
        {
            var viewModel = new pom();
            viewModel.Teachers = await _context.Teachers
                  .Include(c => c.Course1)
                  .Include(d => d.Course2)
                .ThenInclude(i => i.Enrollments)
                    .ThenInclude(i => i.Student)
                .AsNoTracking()
                .ToListAsync();

            if (id != null)
            {
                ViewData["FirstTeacherId"] = id.Value;
                viewModel.Courses = viewModel.Teachers.Where(
                    i => i.TeacherId == id).Single().Course1;
            }
            if (id != null)
            {
                ViewData["SecondTeacherID"] = id.Value;
                viewModel.Courses = viewModel.Teachers.Where(
                    i => i.TeacherId == id).Single().Course2;
            }

            if (courseID != null)
            {
                ViewData["CourseID"] = courseID.Value;
                viewModel.Enrollments = viewModel.Courses.Where(
                    x => x.CourseID == courseID).Single().Enrollments;
            }

            ViewData["CurrentFilter"] = search;
            var teachers = from t in _context.Teachers
                     .Include(c => c.Course1)
                     .Include(d => d.Course2)
                           select t;
            if (!String.IsNullOrEmpty(search))
            {
                teachers = teachers.Where(s => s.LastName.Contains(search)
                                         ||s.FirstName.Contains(search)
                                       || s.Degree.Contains(search)
                                       || s.AcademicRank.Contains(search)
                                       );
                viewModel.Teachers = await teachers.AsNoTracking().ToListAsync();
            }

            return View(viewModel);

        }

        // GET: Teachers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers
                .Include(c => c.Course1)
                  .Include(d => d.Course2)
                .FirstOrDefaultAsync(m => m.TeacherId == id);
            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        // GET: Teachers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Teachers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TeacherId,FirstName,LastName,Degree,AcademicRank,OfficeNumber,HireDate")] Teacher teacher, string[] selectedCourses)
        {
            if (selectedCourses != null)
            {
                teacher.Course1 = new List<Course>();
                foreach (var course in selectedCourses)
                {
                    var courseToAdd = new Course { FirstTeacherId = teacher.TeacherId, CourseID = int.Parse(course) };
                    teacher.Course1.Add(courseToAdd);
                }
                teacher.Course2 = new List<Course>();
                foreach (var course in selectedCourses)
                {
                    var courseToAdd = new Course { SecondTeacherId = teacher.TeacherId, CourseID = int.Parse(course) };
                    teacher.Course2.Add(courseToAdd);
                }
            }
            if (ModelState.IsValid)
            {
                _context.Add(teacher);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(teacher);
        }

        // GET: Teachers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers
              .Include(c => c.Course1)
                .Include(d => d.Course2)
              .ThenInclude(i => i.Enrollments)
              .ThenInclude(i => i.Student)
              .AsNoTracking()
              .FirstOrDefaultAsync(m => m.TeacherId == id);
            if (teacher == null)
            {
                return NotFound();
            }
            return View(teacher);
        }

        // POST: Teachers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacherToUpdate = await _context.Teachers
                .FirstOrDefaultAsync(c => c.TeacherId == id);

            if (await TryUpdateModelAsync<Teacher>(teacherToUpdate,
                "",
                c => c.FirstName, c=>c.LastName, c => c.Degree, c => c.AcademicRank, c => c.OfficeNumber, c => c.HireDate))
            {
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException /* ex */)
                {
                    //Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
                return RedirectToAction(nameof(Index));
            }

            return View(teacherToUpdate);
        }

        // GET: Teachers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(m => m.TeacherId == id);
            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        // POST: Teachers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Teacher teacher = await _context.Teachers
             .Include(i => i.Course1)
             .Include(i => i.Course2)
             .SingleAsync(i => i.TeacherId == id);

            var course = await _context.Courses
                .Where(d => d.FirstTeacherId == id)
                .Where(d => d.SecondTeacherId == id)
                .ToListAsync();
            course.ForEach(d => d.FirstTeacherId = 0);
            course.ForEach(d => d.SecondTeacherId = 0);

            _context.Teachers.Remove(teacher);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TeacherExists(int id)
        {
            return _context.Teachers.Any(e => e.TeacherId == id);
        }
        // GET: Teachers/Courses/2
        public async Task<IActionResult> Courses(int id)
        {
            var courses = _context.Courses.Where(c => c.FirstTeacherId == id || c.SecondTeacherId == id);
            courses = courses.Include(t => t.FirstTeacher).Include(t => t.SecondTeacher);

            ViewData["TeacherId"] = id;
            ViewData["TeacherAcademicRank"] = _context.Teachers.Where(t => t.TeacherId == id).Select(t => t.AcademicRank).FirstOrDefault();
            ViewData["TeacherName"] = _context.Teachers.Where(t => t.TeacherId == id).Select(t => t.FullName).FirstOrDefault();
            return View(courses);
        }
    }
}
