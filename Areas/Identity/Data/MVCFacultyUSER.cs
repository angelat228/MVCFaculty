using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCFaculty.Areas.Identity.Data
{
    public class MVCFacultyUSER : IdentityUser
    {
        public int? StudentId { get; set; }

        public int? TeacherId { get; set; }
    }
}
