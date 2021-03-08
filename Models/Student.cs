using System;
using System.Collections.Generic;

#nullable disable

namespace TestingSystem.Models
{
    public partial class Student
    {
        public Student()
        {
            StudentAnswers = new HashSet<StudentAnswer>();
        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public virtual ICollection<StudentAnswer> StudentAnswers { get; set; }
    }
}
