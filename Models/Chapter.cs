using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestingSystem.Models
{
    public class Chapter
    {
        public Chapter()
        {
            Questions = new HashSet<Question>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
    }
}
