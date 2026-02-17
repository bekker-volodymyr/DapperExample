using System;
using System.Collections.Generic;
using System.Text;

namespace DapperIntro.Entities
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        
        public long AuthorId { get; set; }
        public Author Author { get; set; } = null!;

        public override string ToString()
        {
            return $"{Id}. {Title}. {Author}";
        }
    }
}
