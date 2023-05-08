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
using WebApplicationClassWork.Services;

namespace WebApplicationClassWork.API
{
    [Route("api/article")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly IntroContext _context;
        private readonly Services.IAuthService _authService;

        public ArticleController(IntroContext context, IAuthService authService)
        {
            _context = context; 
            _authService = authService;
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

            DateTime now = DateTime.Now;

            articleToAdd.Text = article.Text;
            articleToAdd.TopicId = TopicId;
            articleToAdd.CreatedDate = now;
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

            _context.Topics.Find(articleToAdd.TopicId).LastArticleMoment = now;

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

            //var list = _context.Articles.Include(a => a.Author).Include(a => a.Topic).Include(a => a.Reply).Where(a => a.TopicId == TopicId).OrderBy(a => a.CreatedDate).ToList();

            return _context.Articles.Include(a => a.Author).Include(a => a.Topic).Include(a => a.Reply).Where(a => a.TopicId == TopicId && a.DeleteMoment == null).OrderBy(a => a.CreatedDate); // возвращаем все статьи топика
        }

        [HttpGet]
        public IEnumerable Get()
        {
            // GET-параметры, передаваемые в запросе (после ?)
            // собираются в коллекции HttpContext.Request.Query
            // доступны через индексатор Query["key"]
            if (HttpContext.Request.Query["del"] == "true")
            {
                // запрос удаленных статей
                // проверяем аутентификацию
                if (_authService.User == null)
                {
                    HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return new string[0];
                }

                return _context.Articles.Where(a =>
                    _authService.User.Id == a.AuthorId
                    &&
                    a.DeleteMoment != null
                ).Include(a => a.Topic);
            }

            return new string[0];
        }

        [HttpDelete("{id}")]
        public object Delete(string id)
        {
            // Проверка аутентификации
            if (_authService.User == null)
            {
                return new { status = "Error", message = "Anauthorized" };
            }

            // Проверка id на валидность
            Guid articleId;
            try
            {
                articleId = Guid.Parse(id);
            }
            catch
            {
                return new { status = "Error", message = "Invalid id" };
            }
            var article = _context.Articles.Find(articleId);
            if (article == null)
            {
                return new { status = "Error", message = "Invalid article" };
            }

            // Проверка того что удаляемый пост принадлежит автору (авторизация)
            if (article.AuthorId != _authService.User.Id)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                return new { status = "Error", message = "Forbidden" };
            }

            // удаление - установка DeleteMoment для статьи
            article.DeleteMoment = DateTime.Now;
            _context.SaveChanges();

            return new { status = "Ok", message = "Deleted" };
        }

        [HttpPut("{id}")]
        public object Put(string id, [FromBody] string text)
        {
            // text не пустой
            if (String.IsNullOrEmpty(text))
            {
                HttpContext.Response.StatusCode =
                    StatusCodes.Status400BadRequest;
                return new
                {
                    Status = "error",
                    message = "Empty text not allowed"
                };
            }
            // id валидный
            Guid articleId;
            try
            {
                articleId = Guid.Parse(id);
            }
            catch
            {
                HttpContext.Response.StatusCode =
                    StatusCodes.Status400BadRequest;
                return new
                {
                    Status = "error",
                    message = "Invalid id format (GUID required)"
                };
            }
            var article = _context.Articles.Find(articleId);
            if (article == null)
            {
                HttpContext.Response.StatusCode =
                    StatusCodes.Status404NotFound;
                return new
                {
                    Status = "error",
                    message = "Article not found"
                };
            }
            if (article.DeleteMoment != null)
            {
                HttpContext.Response.StatusCode =
                    StatusCodes.Status403Forbidden;
                return new
                {
                    Status = "error",
                    message = "Deleted Article should not be edited"
                };
            }
            // авторизация
            if (_authService.User == null)
            {
                HttpContext.Response.StatusCode =
                    StatusCodes.Status401Unauthorized;
                return new
                {
                    Status = "error",
                    message = "Log in for editing"
                };
            }
            // юзер - автор
            if (_authService.User.Id != article.AuthorId)
            {
                HttpContext.Response.StatusCode =
                    StatusCodes.Status403Forbidden;
                return new
                {
                    Status = "error",
                    message = "Only authors could edit articles"
                };
            }
            // фиксируем изменения
            article.Text = text;
            _context.SaveChanges();
            return new
            {
                Status = "Ok",
                message = "Edit complete"
            };
        }

        // Public метод без атрибутов - как метод по умолчанию,
        // сюда попадут запросы, которые не подошли под остальные методы
        // Такой метод должен быть только один (иначе исключение - 
        //  неоднозначность выбора метода), название произвольное (не
        //  обязательно Default)
        public object Default([FromQuery] string uid)
        {
            // Метод запроса можно узнать в HttpContext.Request.Method
            // сюда попадают все методы, в т.ч. нестандартные
            switch (HttpContext.Request.Method)
            {
                case "PURGE": return Purge(uid);
                case "UNLINK": return Unlink(uid);
            }
            return new { uid, HttpContext.Request.Method };
        }

        // Приватные методы не сканнируются контекстом, но могут быть
        // вызваны из метода по умолчанию
        private object Purge(string uid)
        {
            String userId = HttpContext.Request.Headers["User-Id"].ToString();
            // Валидность uid,userId            
            var error = "Error";
            var message = "";
            Guid articleId, authorId;
            try
            {
                articleId = Guid.Parse(uid);
                authorId = Guid.Parse(userId);
            }
            catch
            {
                message = "Invalid id structure";
                return new { error, message };
            }
            // наличие в БД этих id
            var author = _context.Users.Find(authorId);
            if (author == null)
            {
                message = "Author not found";
                return new { error, message };
            }
            var article = _context.Articles.Find(articleId);
            if (article == null)
            {
                message = "Article not found";
                return new { error, message };
            }

            // статья действительно удаленная
            if (article.DeleteMoment == null)
            {
                message = "Article not deleted";
                return new { error, message };
            }

            // соответствие автора статьи и юзера
            if (article.AuthorId != authorId)
            {
                message = "Author rejected";
                return new { error, message };
            }

            // Восстанавливаем статью
            article.DeleteMoment = null;
            _context.SaveChanges();

            message = "Ok";
            return new { message };
        }

        private object Unlink(string uid)
        {
            return new { uid, Method = "Unlink" };
        }


    }


}
