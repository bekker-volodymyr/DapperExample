using System;
using System.Collections.Generic;
using System.Text;

namespace DapperIntro.Entities
{
    public class BookLoan
    {
        public int BookId { get; set; }
        public int VisitorId { get; set; }

        public Visitor Visitor { get; set; } = null!;
        public Book Book { get; set; } = null!;

        public DateTime LoanDate { get; set; }
        public DateTime? ReturnDate { get; set; }
    }
}
