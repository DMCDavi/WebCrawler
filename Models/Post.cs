using System;
using System.Collections.Generic;
using System.Text;

namespace WebCrawler.Models
{
    public class Post
    {
        public long PostId { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }

    }
}
