using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestingSystem.Models
{
    public class Course
    {
        public Course()
        {
            Chapters = new HashSet<Chapter>();
        }
        public int Id { get; set; }
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public int ChaptersCount { get; set; }
        public virtual ICollection<Chapter> Chapters { get; set; }

    }
}
