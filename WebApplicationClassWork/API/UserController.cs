using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using WebApplicationClassWork.DAL.Context;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApplicationClassWork.API
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IntroContext _context;
        private readonly Services.IHasher _hasher;

        public UserController(IntroContext context, Services.IHasher hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        // GET: api/<UserController>
        [HttpGet]
        public string Get(string login, string password)
        {
            if(string.IsNullOrEmpty(login))
            {
                HttpContext.Response.StatusCode = 409;
                return "Conflict: login required";
            }

            if (string.IsNullOrEmpty(password))
            {
                HttpContext.Response.StatusCode = 409;
                return "Conflict: password required";
            }

            DAL.Entities.User user = _context.Users.Where(u => u.Login == login).FirstOrDefault();

            if (user == null)
            {
                HttpContext.Response.StatusCode = 401;
                return "Unauthorized: credentials rejected";
            }

            string PassHash = _hasher.Hash(password + user.PassSalt);

            if (PassHash != user.PassHash)
            {
                HttpContext.Response.StatusCode = 401;
                return "Unauthorized: credentials invalid";
            }

            return user.Id.ToString();
        }

        // GET api/<UserController>/5
        [HttpGet("{id}")]
        public object Get(string id)
        {
            Guid guid;

            try
            {
                guid = Guid.Parse(id);
            }
            catch (Exception ex)
            {
                HttpContext.Response.StatusCode = 409;
                return "Conflict: invalid id format (GUID required)";
            }

            var user = _context.Users.Find(guid);

            if (user != null) return user with { PassHash = "*", PassSalt = "*" };

            return "null";

            //return (_context.Users.Find(guid) ?? new DAL.Entities.User()) with { PassHash = "*", PassSalt = "*"};
        }

        // POST api/<UserController>
        [HttpPost]
        public string Post([FromBody] string value)
        {
            return $"POST {value}";
        }

        // PUT api/<UserController>/5
        [HttpPut("{id}")]
        public object Put(string id, [FromForm] Models.RegUserModel userData)
        {
            Guid userid;
            string fileName = null;

            try
            {
                userid = Guid.Parse(id);
            }
            catch (Exception ex)
            {
                HttpContext.Response.StatusCode = 409;
                return "Conflict: invalid id format (GUID required)";
            }

            var user = _context.Users.Find(userid);

            if (user == null)
            {
                HttpContext.Response.StatusCode = 404;
                return "User does not exists";
            }

            if (userData.Password1 != null && userData.Password2 != null)
            {
                if (!userData.Password1.Equals(userData.Password2))
                {
                    HttpContext.Response.StatusCode = 409;
                    return "Conflict: Passwords are not the same";             
                }
            }

            if (userData.RealName != null)
            {
                if (!Regex.IsMatch(userData.RealName, @"^[A-Z][a-z]+ [A-Z][a-z]+$") && !Regex.IsMatch(userData.RealName, @"^[A-Z][a-z]+$"))  
                {
                    HttpContext.Response.StatusCode = 409;
                    return "Conflict: Wrong Real Name";
                }          
            }

            if (userData.Login != null)
            {
                if(Regex.IsMatch(userData.Login, @"\s"))
                {
                    HttpContext.Response.StatusCode = 409;
                    return "Conflict: Login could not contain spaces";
                }
                else if (_context.Users.Where(u => u.Login == userData.Login).Count() > 0)
                {
                    HttpContext.Response.StatusCode = 409;
                    return "Conflict: Login in use";
                }
            }

            if(userData.Avatar != null)
            {
                string extension = Path.GetExtension(userData.Avatar.FileName);
                fileName = userData.Avatar.FileName.Replace(extension, "-") + Guid.NewGuid() + extension;

                var file = new FileStream("./wwwroot/img/UserImg/" + fileName, FileMode.Create);
                userData.Avatar.CopyToAsync(file).ContinueWith(t => file.Dispose());

                if (user.Avatar != null)
                {
                    System.IO.File.Delete("./wwwroot/img/UserImg/" + user.Avatar);
                }
            }

            if(userData.Email != null)
            {
                if(!Regex.IsMatch(userData.Email, @"^[A-z][A-z\d_]{3,16}@([a-z]{1,10}\.){1,5}[a-z]{2,3}$"))
                {
                    HttpContext.Response.StatusCode = 409;
                    return "Conflict: Wrong Email format";
                }
            }

            if (userData.RealName != null)
            {
                user.RealName = userData.RealName;
            }

            if (userData.Login != null)
            {
                user.Login = userData.Login;
            }

            if (userData.Email != null)
            {
                user.Email = userData.Email;
            }

            if (userData.Avatar != null)
            {
                user.Avatar = fileName;
            }

            if (userData.Password1 != null && userData.Password2 != null)
            {
                user.PassHash = _hasher.Hash(userData.Password1 + user.PassSalt);
            }
            
            _context.SaveChanges();

            return user with { PassHash = "*", PassSalt = "*" }; 
        }

        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
        public string Delete(int id)
        {
            return $"Delete {id}";
        }
    }
}
