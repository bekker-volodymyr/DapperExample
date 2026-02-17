using System;
using System.Collections.Generic;
using System.Text;

namespace DapperIntro.Entities
{
    public class Visitor
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public DateTime BirthDate { get; set; }

        public VisitorPassport Passport { get; set; } = null!;
    }
}
