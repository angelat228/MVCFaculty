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

namespace University.Controllers
{
    public class CoursesController : Controller
    {
        private readonly MVCFacultyContext _context;

        public CoursesController(MVCFacultyContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> GetCoursesByTeacher(int id)
        {
            var teacher = _context.Teachers.Where(m => m.TeacherId == id).FirstOrDefault();
            ViewData["teacherFullName"] = teacher.FullName;
            TempData["selectedTeacher"] = id.ToString();
            var courses = _context.Courses.Where(s => s.FirstTeacherId == id || s.SecondTeacherId == id);
            courses = courses.Include(c => c.FirstTeacher).Include(c => c.SecondTeacher);
            return View(courses);
        }


        // GET: Courses

        public async Task<IActionResult> Index(string CourseSemester, string CourseProgramme, string SearchString)
        {
            IQueryable<Course> courses = _context.Courses.AsQueryable();
            IQueryable<int> semesterQuery = _context.Courses.OrderBy(m => m.Semester).Select(m => m.Semester).Distinct();
            IQueryable<string> programmeQuery = _context.Courses.OrderBy(m => m.Programme).Select(m => m.Programme).Distinct();

            if (!string.IsNullOrEmpty(SearchString))
            {
                courses = courses.Where(s => s.Title.ToLower().Contains(SearchString.ToLower()));
            }
            int CourseSemesterID = Convert.ToInt32(CourseSemester);
            if (CourseSemesterID != 0)
            {
                courses = courses.Where(x => x.Semester == CourseSemesterID);
            }
            if (!string.IsNullOrEmpty(CourseProgramme))
            {
                courses = courses.Where(x => x.Programme == CourseProgramme);
            }

            courses = courses.Include(c => c.FirstTeacher).Include(c => c.SecondTeacher)
                  .Include(c => c.Enrollments).ThenInclude(c => c.Student);


            var TitleSemesterProgramme = new TitleSemesterProgrammeViewModel
            {
                Semesters = new SelectList(await semesterQuery.ToListAsync()),
                Programmes = new SelectList(await programmeQuery.ToListAsync()),
                Courses = await courses.ToListAsync()
            };

            return View(TitleSemesterProgramme);
        }



        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(s => s.Enrollments)
            .ThenInclude(e => e.Student)
         .Include(c => c.FirstTeacher)
                .Include(p => p.SecondTeacher)
                .FirstOrDefaultAsync(m => m.CourseID == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }
        public IActionResult Create()
        {
            ViewData["FirstTeacherId"] = new SelectList(_context.Teachers, "TeacherId", "FullName");
            ViewData["SecondTeacherId"] = new SelectList(_context.Teachers, "TeacherId", "FullName");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create([Bind("CourseID,Title,Credits,Semestar,Programme,EducationLevel,FirstTeacherId,SecondTeacherId")] Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FirstTeacherID"] = new SelectList(_context.Set<Teacher>(), "Id", "FullName", course.FirstTeacherId);
            ViewData["SecondTeacherID"] = new SelectList(_context.Set<Teacher>(), "Id", "FullName", course.SecondTeacherId);
            return View(course);
        }
        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            {
                String s = null;
                if (TempData["test"] != null)
                    s = TempData["test"].ToString();
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

                ViewData["FirstTeacherId"] = new SelectList(_context.Teachers, "TeacherId", "FullName", course.FirstTeacherId);
                ViewData["SecondTeacherId"] = new SelectList(_context.Teachers, "TeacherId", "FullName", course.SecondTeacherId);
                return View(course);
            }
        }


        // POST: Courses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CourseID,Title,Credits,Semestar,Programme,EducationLevel,FirstTeacherId,SecondTeacherId")] Course course)
        {
            String s = null;
            if (TempData["test"] != null)
                s = TempData["test"].ToString();
            if (id != course.CourseID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.CourseID))
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
            ViewData["FirstTeacherId"] = new SelectList(_context.Teachers, "TeacherId", "FirstName", course.FirstTeacherId);
            ViewData["SecondTeacherId"] = new SelectList(_context.Teachers, "TeacherId", "FirstName", course.SecondTeacherId);
            return View(course);
        }

     
        // GET:  Courses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.FirstTeacher)
                .Include(p => p.SecondTeacher)
                .Include(e => e.Enrollments)
                .ThenInclude(e => e.Student)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.CourseID == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.CourseID == id);
        }
       

    }
}