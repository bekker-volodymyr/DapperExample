using System;
using System.Collections.Generic;
using System.Text;

namespace DapperIntro.Entities
{
    public class Employee
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public decimal Salary { get; set; }

    }
}
