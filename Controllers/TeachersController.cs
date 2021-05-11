using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
    public class TeachersController : Controller
    {
        private readonly MVCFacultyContext _context;
        private  IWebHostEnvironment _webHostEnvironment { get;  }

        public TeachersController(MVCFacultyContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Teachers
        public async Task<IActionResult> Index(long? id, long? courseID, string search)
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
        public async Task<IActionResult> Details(long? id)
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
        public async Task<IActionResult> Create(TeacherForm Vmodel)
        {
           
            if (ModelState.IsValid)
            {


                string uniqueFileName = UploadedFile(Vmodel);

                Teacher teacher = new Teacher
                {
                    ProfilePicture = uniqueFileName,
                    FirstName = Vmodel.FirstName,
                    LastName = Vmodel.LastName,
                    Degree = Vmodel.Degree,
                    AcademicRank = Vmodel.AcademicRank,
                    OfficeNumber = Vmodel.OfficeNumber,
                    HireDate = Vmodel.HireDate,

                    Course1 = Vmodel.Courses_first,
                    Course2 = Vmodel.Courses_second
                };

                _context.Add(teacher);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
                return View();
            }

        private string UploadedFile(TeacherForm Vmodel)
        {
            string uniqueFileName = null;

            if (Vmodel.ProfilePicture != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(Vmodel.ProfilePicture.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    Vmodel.ProfilePicture.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }

        // GET: Teachers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers.FindAsync(id);

            if (teacher == null)
            {
                return NotFound();
            }

            TeacherForm vm = new TeacherForm
            {
                Id = teacher.TeacherId,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                Degree = teacher.Degree,
                AcademicRank = teacher.AcademicRank,
                OfficeNumber = teacher.OfficeNumber,
                HireDate = teacher.HireDate,
                Courses_first = teacher.Course1,
                Courses_second = teacher.Course2
            };

            return View(vm);

        }
        // POST: Teachers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TeacherForm Vmodel)
             
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

                    Teacher teacher = new Teacher
                    {
                        ProfilePicture = uniqueFileName,
                        TeacherId = Vmodel.Id,
                        FirstName = Vmodel.FirstName,
                        LastName = Vmodel.LastName,
                        Degree = Vmodel.Degree,
                        AcademicRank = Vmodel.AcademicRank,
                        OfficeNumber = Vmodel.OfficeNumber,
                        HireDate = Vmodel.HireDate,
                        Course1 = Vmodel.Courses_first,
                        Course2 = Vmodel.Courses_second
                    };
                    _context.Update(teacher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherExists(Vmodel.Id))
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
             var teacher = await _context.Teachers.FindAsync(id);

             _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
        private bool TeacherExists(int id)
        {
            return _context.Teachers.Any(e => e.TeacherId == id);
        }

    }
}
