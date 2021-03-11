using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestingSystem.Models;

namespace TestingSystem.ViewModels
{
    public class FinishedViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<StudentAnswer> StudentAnswers { get; set; }
    }
}
