using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVCFaculty.Data;
using MVCFaculty.Models;

namespace MVCFaculty.Controllers
{
    public class StudentsController : Controller
    {
        private readonly MVCFacultyContext _context;

        public StudentsController(MVCFacultyContext context)
        {
            _context = context;
        }

        // GET: Students
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
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,StudentId,FirstName,LastName,EnrollmentDate,AcquiredCredits,CurrentSemestar,EducationLevel")] Student student)
        {
            if (ModelState.IsValid)
            {
                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Students/Edit/5
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
            return View(student);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var studentToUpdate = await _context.Students.FirstOrDefaultAsync(s => s.ID == id);
            if (await TryUpdateModelAsync<Student>(
                studentToUpdate,
                "",
                s => s.StudentId, s => s.FirstName, s => s.LastName, s => s.EnrollmentDate, s => s.AcquiredCredits, s => s.CurrentSemestar, s => s.EducationLevel))
            {
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
            }
            return View(studentToUpdate);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(long? id)
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

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.)
                return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });
            }
        }

        private bool StudentExists(long id)
        {
            return _context.Students.Any(e => e.ID == id);
        }


        public async Task<IActionResult> StudentsByCourse(long? id)
        {
            IQueryable<Course> courses = _context.Courses.Include(c => c.FirstTeacher).Include(c => c.SecondTeacher).AsQueryable();

            IQueryable<Enrollment> enrollments = _context.Enrollments.AsQueryable();
            enrollments = enrollments.Where(s => s.StudentID == id); //se zemaat onie zapisi kaj koi studentId == id-to od url-to
            IEnumerable<int> enrollmentsIDS = enrollments.Select(e => e.CourseID).Distinct(); //se zemaat distinct IDs na courses od prethodno najdenite zapisi

            courses = courses.Where(s => enrollmentsIDS.Contains(s.CourseID));  //filtriranje na students spored id

            courses = courses.Include(c => c.Enrollments).ThenInclude(c => c.Student);

            ViewData["StudentName"] = _context.Students.Where(t => t.ID == id).Select(t => t.FullName).FirstOrDefault();
            ViewData["studentId"] = id;
            return View(courses);
        }

    }
}
