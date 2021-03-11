using System;
using System.Collections.Generic;

#nullable disable

namespace TestingSystem.Models
{
    public partial class StudentAnswer
    {
        public long Id { get; set; }
        public int? StudentId { get; set; }
        public int? QuestionId { get; set; }
        public DateTime Date { get; set; }
        public short Point { get; set; }
        public string Answer { get; set; }
        public virtual Question Question { get; set; }
        public virtual Student Student { get; set; }
    }
}
