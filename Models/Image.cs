using System;
using System.Collections.Generic;
using System.Text;

namespace WebCrawler.Models
{
    public class Image
    {
        public long ImageId { get; set; }
        public long PostId { get; set; }
        public virtual Post Post { get; set; }
        public string Path { get; set; }
        public string ImgTitle { get; set; }
        public string Description { get; set; }
    }
}
