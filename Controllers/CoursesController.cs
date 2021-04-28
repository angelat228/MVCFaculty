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
        .AsNoTracking()
                .FirstOrDefaultAsync(m => m.CourseID == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }
        public IActionResult Create()
        {
            PopulateTeachersDropDownList1();
            PopulateTeachersDropDownList2();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseID, Title, Credits, Semester, Programme, EducationLevel, FirstTeacherId, SecondTeacherId")] Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateTeachersDropDownList1(course.FirstTeacherId);
            PopulateTeachersDropDownList2(course.SecondTeacherId);

            return View(course);
        }


        // GET: Courses/Edit/5
         public async Task<IActionResult> Edit(int? id)
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
              IEnumerable<Student> students = _context.Students.AsEnumerable();

              ViewModel viewmodel = new ViewModel
              {
                  course = course,
                  studentList = new MultiSelectList(students.AsEnumerable(), "ID", "FullName"),
                  selectedStudents = (IEnumerable<long>)course.Enrollments.Select(m => m.StudentID)

              };
              PopulateTeachersDropDownList1(course.FirstTeacherId);
              PopulateTeachersDropDownList2(course.SecondTeacherId);
              return View(viewmodel);
          }

        // POST: Courses/Edit/5

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ViewModel viewmodel)
        {
            if (id != viewmodel.course.CourseID)
            {
                return NotFound();
            }

            try
            {
                    _context.Update(viewmodel.course);
                    await _context.SaveChangesAsync();
                    IEnumerable<long> listStudents = viewmodel.selectedStudents;
                    IQueryable<Enrollment> toBeRemoved = _context.Enrollments.Where((s => !listStudents.Contains(s.StudentID) && s.CourseID == id));
                    _context.Enrollments.RemoveRange(toBeRemoved);

                    IEnumerable<long> existStudents = _context.Enrollments
                        .Where(s => listStudents.Contains(s.StudentID) && s.CourseID == id).Select(s => s.StudentID);
                    IEnumerable<long> newStudents = listStudents.Where(s => !existStudents.Contains(s));
                    foreach (int studentID in newStudents)
                        _context.Enrollments.Add(new Enrollment { StudentID = studentID, CourseID = id });
                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(viewmodel.course.CourseID))
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

        
        private void PopulateTeachersDropDownList1(object selectedTeacher = null)
        {
            var teachersQuery = from d in _context.Teachers
                                orderby d.FirstName
                                select d;
            ViewBag.FirstTeacherId = new SelectList(teachersQuery.AsNoTracking(), "TeacherId", "FullName", selectedTeacher);
        }
        private void PopulateTeachersDropDownList2(object selectedTeacher = null)
        {
            var teachersQuery = from d in _context.Teachers
                                orderby d.FirstName
                                select d;
            ViewBag.SecondTeacherId = new SelectList(teachersQuery.AsNoTracking(), "TeacherId", "FullName", selectedTeacher);
        }

        // GET: Courses/Delete/5
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



        [HttpPost]

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.CourseID == id);
        }

    }
}