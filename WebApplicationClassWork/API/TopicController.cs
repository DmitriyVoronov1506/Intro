using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using WebApplicationClassWork.DAL.Entities;
using System.Linq;
using WebApplicationClassWork.DAL.Context;
using WebApplicationClassWork.Models;
using System.Collections.Generic;

namespace WebApplicationClassWork.API
{
    [Route("api/topic")]
    [ApiController]
    public class TopicController : ControllerBase
    {
        private readonly IntroContext _context;

        public TopicController(IntroContext context)
        {
            _context = context;
        }


        [HttpPost]
        public object Post(TopicModel topic)
        {
            string UserIdHeader = HttpContext.Request.Headers["User-Id"].ToString();

            Guid UserId;

            try
            {
                UserId = Guid.Parse(UserIdHeader);
            }
            catch(Exception ex)
            {
                return new
                {
                    status = "Error",
                    message = "User Id header empty ot invalid (GUID expected)"
                };
            }

            var SupportedCultures = new string[] { "uk-ua", "en-gb" };

            string CultureHeaders = HttpContext.Request.Headers["Culture"].ToString();

            if(Array.IndexOf(SupportedCultures, CultureHeaders) == -1)
            {
                return new
                {
                    status = "Error",
                    message = "Culture header invalid or not supported"
                };
            }

            HttpContext.Response.Headers.Add("Culture", CultureHeaders);

            if (topic == null)
            {
                return new
                {
                    status = "Error",
                    message = "No data"
                };
            }

            if(string.IsNullOrEmpty(topic.Title) || string.IsNullOrEmpty(topic.Description))
            {
                return new
                {
                    status = "Error",
                    message = "Empty title or description"
                };
            }

            var user = _context.Users.Find(UserId);

            if(user == null)
            {
                return new
                {
                    status = "Error",
                    message = "Forbidden"
                };
            }

            if(_context.Topics.Where(t => t.Title == topic.Title).Any())
            {
                return new
                {
                    status = "Error",
                    message = $"Topic '{topic.Title}' exists"
                };
            }

            _context.Topics.Add(new()
            {
                Title = topic.Title,
                Description = topic.Description,
                AuthorId = UserId
            });

            _context.SaveChangesAsync();

            return new { status = "Ok", message = $"Topic '{topic.Title}' created" };
        }

        [HttpGet]
        public IEnumerable<Topic> Get()
        {
            return _context.Topics;
        }
    }
}
