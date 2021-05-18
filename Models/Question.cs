using System;
using System.Collections.Generic;

#nullable disable

namespace TestingSystem.Models
{
    public partial class Question
    {
        public Question()
        {
            StudentAnswers = new HashSet<StudentAnswer>();
        }

        public int Id { get; set; }
        public string Text { get; set; }
        public string Answer { get; set; }
		public string Variant1 { get; set; }
        public string Variant2 { get; set; }
        public string Variant3 { get; set; }
        public double Difficulty { get; set; }
        public ques_type ques_type { get; set; }

        public virtual ICollection<StudentAnswer> StudentAnswers { get; set; }
    }
}
