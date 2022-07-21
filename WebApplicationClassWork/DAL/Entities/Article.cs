using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplicationClassWork.DAL.Entities
{
    public class Article
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public Guid TopicId { get; set; }
        public string Text { get; set; }
        public Guid AuthorId { get; set; }
        public Guid? ReplyId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string PictureFile { get; set; }
    }
}
