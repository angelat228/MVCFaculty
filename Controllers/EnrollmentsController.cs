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
    public class EnrollmentsController : Controller
    {
        private readonly MVCFacultyContext _context;

        public EnrollmentsController(MVCFacultyContext context)
        {
            _context = context;
        }

        // GET: Enrollments
        public async Task<IActionResult> Index()
        {
            var mVCFacultyContext = _context.Enrollments.Include(e => e.Course).Include(e => e.Student);
            return View(await mVCFacultyContext.ToListAsync());
        }

        // GET: Enrollments/Details/5
        public async Task<IActionResult> Details(long? id)
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

        // GET: Enrollments/Create
        public IActionResult Create()
        {
            ViewData["CourseID"] = new SelectList(_context.Courses, "CourseID", "Title");
            ViewData["StudentID"] = new SelectList(_context.Students, "ID", "FirstName");
            return View();
        }

        // POST: Enrollments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EnrollmentID,CourseID,StudentID,Semester,Year,Grade,SeminalUrl,ProjectUrl,ExamPoints,SeminalPoints,ProjectPoints,AdditionalPoints,FinishDate")] Enrollment enrollment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(enrollment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseID"] = new SelectList(_context.Courses, "CourseID", "Title", enrollment.CourseID);
            ViewData["StudentID"] = new SelectList(_context.Students, "ID", "FirstName", enrollment.StudentID);
            return View(enrollment);
        }

        // GET: Enrollments/Edit/5
        public async Task<IActionResult> Edit(long? id)
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
            ViewData["CourseID"] = new SelectList(_context.Courses, "CourseID", "Title", enrollment.CourseID);
            ViewData["StudentID"] = new SelectList(_context.Students, "ID", "FirstName", enrollment.StudentID);
            return View(enrollment);
        }

        // POST: Enrollments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("EnrollmentID,CourseID,StudentID,Semester,Year,Grade,SeminalUrl,ProjectUrl,ExamPoints,SeminalPoints,ProjectPoints,AdditionalPoints,FinishDate")] Enrollment enrollment)
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseID"] = new SelectList(_context.Courses, "CourseID", "Title", enrollment.CourseID);
            ViewData["StudentID"] = new SelectList(_context.Students, "ID", "FirstName", enrollment.StudentID);
            return View(enrollment);
        }

        // GET: Enrollments/Delete/5
        public async Task<IActionResult> Delete(long? id)
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
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EnrollmentExists(long id)
        {
            return _context.Enrollments.Any(e => e.EnrollmentID == id);
        }


        public async Task<IActionResult> StudentCourse(int? id, int enrollmentYear)
        {
            if (id == null)
            {
                return NotFound();
            }

            IQueryable<Enrollment> enrollments = _context.Enrollments.Where(e => e.CourseID == id);
            enrollments = enrollments.Include(e => e.Course).Include(e => e.Student).OrderBy(e => e.Student.StudentId).OrderBy(e => e.Student.StudentId);

            if (enrollmentYear != 0)
            {
                enrollments = enrollments.Where(x => x.Year == enrollmentYear);
            }
            else
            {
                enrollments = enrollments.Where(x => x.Year == DateTime.Now.Year);
            }

            EnrollmentFilter vm = new EnrollmentFilter
            {
                Enrollments = await enrollments.ToListAsync()
            };

            ViewData["CourseName"] = _context.Courses.Where(c => c.CourseID == id).Select(c => c.Title).FirstOrDefault();
            return View(vm);
        }
        // GET: Enrollments/Editstudent/5
        public async Task<IActionResult> Editstudent(long? id)
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

            ViewData["CourseID"] = new SelectList(_context.Courses, "CourseID", "Title", enrollment.CourseID);
            ViewData["ID"] = new SelectList(_context.Students, "ID", "FullName", enrollment.StudentID);
            ViewData["StudentName"] = _context.Students.Where(s => s.ID == enrollment.StudentID).Select(s => s.FullName).FirstOrDefault();
            ViewData["CourseName"] = _context.Courses.Where(s => s.CourseID == enrollment.CourseID).Select(s => s.Title).FirstOrDefault();
            return View(enrollment);
        }

        // POST: Enrollments/Editstudent/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editstudent(long id, [Bind("EnrollmentID,CourseID, StudentID, Semester,Year,Grade,SeminalUrl,ProjectUrl,ExamPoints,SeminalPoints,ProjectPoints,AdditionalPoints,FinishDate")] Enrollment enrollment)
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
                return RedirectToAction("CourseStudents", new { id = enrollment.CourseID });
            }
            ViewData["CourseID"] = new SelectList(_context.Courses, "CourseID", "Title", enrollment.CourseID);
            ViewData["ID"] = new SelectList(_context.Students, "ID", "FullName", enrollment.StudentID);
            return View(enrollment);
        }

        // GET: Enrollments/EditByStudent/5
        public async Task<IActionResult> EditByStudent(long? id)
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

            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseID", "Title", enrollment.CourseID);
            ViewData["StudentId"] = new SelectList(_context.Students, "ID", "FullName", enrollment.StudentID);

            EnrollmentView vm = new EnrollmentView
            {
                Id = enrollment.EnrollmentID,
                Semester = enrollment.Semester,
                Year = enrollment.Year,
                Grade = enrollment.Grade,
                ProjectUrl = enrollment.ProjectUrl,
                SeminalPoints = enrollment.SeminalPoints,
                ProjectPoints = enrollment.ProjectPoints,
                AdditionalPoints = enrollment.AdditionalPoints,
                ExamPoints = enrollment.ExamPoints,
                FinishDate = enrollment.FinishDate,
                CourseId = enrollment.CourseID,
                StudentId = enrollment.StudentID
            };

            ViewData["StudentName"] = _context.Students.Where(s => s.ID == enrollment.StudentID).Select(s => s.FullName).FirstOrDefault();
            ViewData["CourseName"] = _context.Courses.Where(s => s.CourseID == enrollment.CourseID).Select(s => s.Title).FirstOrDefault();
            return View(vm);
        }

        // POST: Enrollments/EditByStudent/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditByStudent(long id, EnrollmentView vm)
        {
            if (id != vm.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {

                    Enrollment enrollment = new Enrollment
                    {
                        EnrollmentID = vm.Id,
                        Semester = vm.Semester,
                        Year = vm.Year,
                        Grade = vm.Grade,
                        SeminalUrl = vm.SeminalUrl,
                        ProjectUrl = vm.ProjectUrl,
                        SeminalPoints = vm.SeminalPoints,
                        ProjectPoints = vm.ProjectPoints,
                        AdditionalPoints = vm.AdditionalPoints,
                        ExamPoints = vm.ExamPoints,
                        FinishDate = vm.FinishDate,
                        CourseID = vm.CourseId,
                        StudentID = vm.StudentId
                    };

                    _context.Update(enrollment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrollmentExists(vm.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", new { id = vm.Id });
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "CourseID", "Title", vm.CourseId);
            ViewData["StudentId"] = new SelectList(_context.Students, "ID", "FullName", vm.StudentId);
            return View(vm);
        }

    }
}
