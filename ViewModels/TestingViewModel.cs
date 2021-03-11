using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestingSystem.Models;

namespace TestingSystem.ViewModels
{
    public class TestingViewModel
    {
        public List<Question> Questions { get; set; }
        public List<StudentAnswer> Answers { get; set; }
    }
}
