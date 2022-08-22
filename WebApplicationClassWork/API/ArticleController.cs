using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WebApplicationClassWork.DAL.Context;
using WebApplicationClassWork.DAL.Entities;
using WebApplicationClassWork.Models;

namespace WebApplicationClassWork.API
{
    [Route("api/article")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly IntroContext _context;

        public ArticleController(IntroContext context)
        {
            _context = context;
        }

        [HttpPost]
        public object Post([FromForm] ArticleModel article) // fromform что ы удобнее через постман посылать картинку
        {
            Guid AuthorId;   // проверяем гуиды на валидность
            Guid TopicId;
            string fileName = string.Empty;

            try
            {
                AuthorId = Guid.Parse(article.AuthorId);
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

            if(_context.Users.Find(AuthorId) == null)
            {
                return new { status = "Error", message = "Invalid Author" };
            }

            var articleToAdd = new Article();  // Валидация пройдена, начинаем собирать статью

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
            
            if(article.PictureFile != null) // Для картинок создал отдельные папки для Юзера и для Статей что бы не смешивать 
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

            return new { status = "Ok", message = $"Article for topic '{article.TopicId}' was created" };  // Статья создана успешно
        }

        [HttpGet("{id}")]
        public IEnumerable Get(string id)
        {
            Guid TopicId;  // Если гуид 

            try
            {
                TopicId = Guid.Parse(id);
            }
            catch (Exception ex)
            {
                return null;
            }

            var list = _context.Articles.Include(a => a.Author).Include(a => a.Topic).Where(a => a.TopicId == TopicId).OrderBy(a => a.CreatedDate).ToList();

            return _context.Articles.Include(a => a.Author).Include(a => a.Topic).Where(a => a.TopicId == TopicId).OrderBy(a => a.CreatedDate); // возвращаем все статьи топика
        }
    }
}
