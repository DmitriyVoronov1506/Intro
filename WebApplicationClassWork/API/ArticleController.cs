using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WebApplicationClassWork.DAL.Context;
using WebApplicationClassWork.DAL.Entities;
using WebApplicationClassWork.Models;

namespace WebApplicationClassWork.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly IntroContext _context;

        public ArticleController(IntroContext context)
        {
            _context = context;
        }

        [HttpPost]
        public object Post(ArticleModel article)
        {
            string AuthorIdHeader = HttpContext.Request.Headers["Author-Id"].ToString();

            Guid AuthorId;
            Guid TopicId;
            string fileName = string.Empty;

            try
            {
                AuthorId = Guid.Parse(AuthorIdHeader);
            }
            catch (Exception ex)
            {
                return new { status = "Error", message = "Author Id header empty or invalid (GUID expected)" };
            }

            try
            {
                TopicId = Guid.Parse(article.TopicId);
            }
            catch (Exception ex)
            {
                return new { status = "Error", message = "Topic Id is empty or invalid (GUID expected)" };
            }

            if(string.IsNullOrEmpty(article.Text))
            {
                return new { status = "Error", message = "Article text is empty" };
            }

            var articleToAdd = new Article();

            if (article.ReplyId == null)
            {
                articleToAdd.ReplyId = null;
            }
            else
            {
                try
                {
                    articleToAdd.ReplyId = Guid.Parse(article.ReplyId);
                }
                catch (Exception ex)
                {
                    return new { status = "Error", message = "Reply Id is empty or invalid (GUID expected)" };
                }
            }

            articleToAdd.Text = article.Text;
            articleToAdd.TopicId = TopicId;
            articleToAdd.CreatedDate = DateTime.Now;
            articleToAdd.AuthorId = AuthorId;
            
            if(article.PictureFile != null)
            {
                bool ifExists = true;

                string extension = Path.GetExtension(article.PictureFile.FileName);  // Получаем только тип файла (.png, .jpg, ...)

                var pictures = new DirectoryInfo("./wwwroot/img/ArticleImg/").GetFiles();  // Получаем все картинки которые есть в папке img

                while (ifExists)
                {
                    fileName = article.PictureFile.FileName.Replace(extension, "-") + Guid.NewGuid() + extension;  // Генерируем новое имя из имени файла, гуида и расширения

                    var finedName = pictures.Where(p => p.Name.Equals(fileName)).FirstOrDefault(); // Проверяем нет ли файла с таким именем (хоть и гуид, но на всякий случай)

                    if (finedName == null) // Если нашли с таким именем, возвращаемся в цикл и генерируем опять имя
                    {
                        article.PictureFile.CopyToAsync(new FileStream("./wwwroot/img/ArticleImg/" + fileName, FileMode.Create));  // Если такого имени нет, создаём картинку

                        ifExists = false;
                    }
                }
            }

            articleToAdd.PictureFile = fileName;

            _context.Articles.Add(articleToAdd);
            _context.SaveChanges();

            return new { status = "Ok", message = $"Article for topic '{article.TopicId}' was created" };
        }

        [HttpGet]
        public IEnumerable<Article> Get(string topicId)
        {
            Guid TopicId;

            try
            {
                TopicId = Guid.Parse(topicId);
            }
            catch (Exception ex)
            {
                return null;
            }

            return _context.Articles.Where(a => a.TopicId == TopicId);
        }
    }
}
