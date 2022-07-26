using Microsoft.AspNetCore.Http;
using System;

namespace WebApplicationClassWork.Models
{
    public class ArticleModel
    {
        public string TopicId { get; set; }
        public string Text { get; set; }
        public string ReplyId { get; set; }
        public IFormFile PictureFile { get; set; }
    }
}
