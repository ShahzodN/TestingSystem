using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestingSystem.Models
{
    public class Group
    {
        public Group()
        {
            Students = new HashSet<Student>();
        }
        public int Id { get; set; }
        public string Number { get; set; }
        public virtual ICollection<Student> Students { get; set; }
    }
}
