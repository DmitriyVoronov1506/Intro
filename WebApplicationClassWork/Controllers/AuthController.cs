using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.RegularExpressions;
using WebApplicationClassWork.Services;

namespace WebApplicationClassWork.Controllers
{
    public class AuthController : Controller
    {
        private readonly Services.IHasher _hasher;
        private readonly RandomService _randomService;
        private readonly DAL.Context.IntroContext _introContext;
        private readonly IAuthService _authService;

        public AuthController(Services.IHasher hasher, DAL.Context.IntroContext introContext, 
            RandomService randomService, IAuthService authService)
        {
            _hasher = hasher;
            _introContext = introContext;
            _randomService = randomService;
            _authService = authService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewData["AuthUser"] = _authService.User; // Берём пользователя из нашего обьекта который получаем в сервис из middlware

            base.OnActionExecuting(context);
        }

        public IActionResult Index()
        {
            string LoginError = HttpContext.Session.GetString("LoginError");

            if (LoginError != null)
            {
                ViewData["LoginError"] = LoginError;
                HttpContext.Session.Remove("LoginError");
            }

            string userId = HttpContext.Session.GetString("userId");

            if (userId != null)
            {
                ViewData["AuthUser"] = _introContext.Users.Find(Guid.Parse(userId));
            }

            ViewData["fromAuthMiddleware"] = HttpContext.Items["fromAuthMiddleware"];

            return View();
        }

        public IActionResult Register()
        {
            String err = HttpContext.Session.GetString("RegError");

            if (err != null)
            {
                ViewData["err"] = err.Split(";");
                ViewData["SaveData"] = HttpContext.Session.GetString("savedata").Split(";");
                HttpContext.Session.Remove("RegError");
                HttpContext.Session.Remove("savedata");
            }

            ViewData["image"] = HttpContext.Session.GetString("pictureName");
            HttpContext.Session.Remove("pictureName");

            string userdata = HttpContext.Session.GetString("UserData");
            
            if(userdata != null)
            {
                Models.RegUserModel USer = JsonConvert.DeserializeObject<Models.RegUserModel>(userdata);
                HttpContext.Session.Remove("UserData");
            }

            return View();
        }

        public RedirectResult LogOut()  // Нажимаем кнопку и убираем сессию с юзером. Авторизация пропадает  
        {
            HttpContext.Session.Remove("userId");

            _authService.User.LogMoment = DateTime.Now;   // Запоминаем момент логина (в миддлвер тоже самое)
            _introContext.SaveChanges();

            return Redirect("/");  // Bозвращаемся на Home
        }

        [HttpPost]
        public RedirectResult Login(string UserLogin, string UserPassword)
        {
            DAL.Entities.User user;

            try
            {
                if (string.IsNullOrEmpty(UserLogin))
                {
                    throw new Exception("Login Empty");
                }

                if (string.IsNullOrEmpty(UserPassword))
                {
                    throw new Exception("Password Empty");                  
                }

                user = _introContext.Users.Where(u => u.Login == UserLogin).FirstOrDefault();

                if (user == null)
                {
                    throw new Exception("Login invalid");
                }

                string PassHash = _hasher.Hash(UserPassword + user.PassSalt);

                if(PassHash != user.PassHash)
                {
                    throw new Exception("Password invalid");
                }
            }
            catch (Exception ex)
            {
                HttpContext.Session.SetString("LoginError", ex.Message);

                return Redirect("/Auth/Index");
            }

            HttpContext.Session.SetString("userId", user.Id.ToString());

            HttpContext.Session.SetString("AuthMoment", DateTime.Now.Ticks.ToString());

            return Redirect("/");
        }

        [HttpPost]
        public IActionResult RegUser(Models.RegUserModel UserData)
        {
            //return Json(UserData);
            string fileName = String.Empty;  // Имя файла, который загружаем
            string[] err = new string[6];
            string[] saveData = new string[3];

            bool failedValidation = false; // Добавим переменную что бы знать когда валидация успешна, а когда нет (Что бы знать когда сохранять данные и возвращать на поля)

            if (UserData == null)
            {
                err[0] = "Некорректный вызов (нет данных)";
            }
            else
            {
                if (String.IsNullOrEmpty(UserData.RealName))
                {
                    err[1] = "Email не может быть пустым";
                    failedValidation = true;
                }

                if (String.IsNullOrEmpty(UserData.Login))
                {
                    err[2] = "Логин не может быть пустым";
                    failedValidation = true;
                }
                else
                {
                    var login = _introContext.Users.Where(u => u.Login.Equals(UserData.Login)).FirstOrDefault();

                    if(login != null)
                    {
                        err[2] = "Пользователь с таким логином уже зарегестрирован!";
                        failedValidation = true;
                    }
                }

                if (String.IsNullOrEmpty(UserData.Email))
                {
                    err[5] = "Email не может быть пустым";
                    failedValidation = true;
                }

                if (!String.IsNullOrEmpty(UserData.Password1))
                {
                    if (!UserData.Password1.Equals(UserData.Password2))
                    {
                        err[4] = "Пароли не совпадают";
                        failedValidation = true;
                    }
                }
                else
                {
                    err[3] = "Пароль не может быть пустым";
                    failedValidation = true;
                }

                if (UserData.Avatar != null)
                {
                    bool ifExists = true;

                    string extension = Path.GetExtension(UserData.Avatar.FileName);  // Получаем только тип файла (.png, .jpg, ...)

                    var pictures = new DirectoryInfo("./wwwroot/img/").GetFiles();  // Получаем все картинки которые есть в папке img

                    while (ifExists)
                    {
                        fileName = UserData.Avatar.FileName.Replace(extension, "-") + Guid.NewGuid() + extension;  // Генерируем новое имя из имени файла, гуида и расширения

                        var finedName = pictures.Where(p => p.Name.Equals(fileName)).FirstOrDefault(); // Проверяем нет ли файла с таким именем (хоть и гуид, но на всякий случай)

                        if (finedName == null) // Если нашли с таким именем, возвращаемся в цикл и генерируем опять имя
                        {
                            UserData.Avatar.CopyToAsync(new FileStream("./wwwroot/img/" + fileName, FileMode.Create));  // Если такого имени нет, создаём картинку

                            ifExists = false;
                        }
                    }
                }

                bool isValid = true;

                foreach (var error in err)
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        isValid = false;
                    }
                }

                if (isValid)
                {
                    var user = new DAL.Entities.User();
                    user.PassSalt = _hasher.Hash(DateTime.Now.ToString());
                    user.PassHash = _hasher.Hash(UserData.Password1 + user.PassSalt);
                    user.Avatar = fileName;
                    user.Email = UserData.Email;
                    user.RealName = UserData.RealName;
                    user.Login = UserData.Login;
                    user.RegMoment = DateTime.Now;

                    _introContext.Users.Add(user);
                    _introContext.SaveChanges();
                }
            }

            //ViewData["err"] = err; //не переживает redirect
            HttpContext.Session.SetString("RegError", String.Join(";", err));

            if (failedValidation)  // Если не прошёл валидацию, берём данные из 3 полей и закидываем в массив. Передаём во ViewData и отображаем в Value html
            {
                saveData[0] = UserData.RealName;
                saveData[1] = UserData.Login;
                saveData[2] = UserData.Email;
            }

            HttpContext.Session.SetString("savedata", String.Join(";", saveData));
            HttpContext.Session.SetString("pictureName", fileName);

            UserData.Password1 = String.Empty;
            UserData.Password1 = String.Empty;
            UserData.Avatar = null;

            HttpContext.Session.SetString("UserData", JsonConvert.SerializeObject(UserData));

            return RedirectToAction("Register");
        }

        public IActionResult Profile()
        {
            if (_authService.User == null)
            {
                //return View("Index"); // Внутренний редирект (адрес auth/profil, а показывает auth/index)

                return Redirect("/Auth/Index"); // Внешний редирект 
            }

            return View();
        }

        public string ChangeRealName(string NewName)
        {
            if(_authService.User == null)
            {
                return "Forbidden";
            }

            Regex NameAndSurname = new Regex(@"^[A-Z][a-z]+ [A-Z][a-z]+$");  // Регулярка в формате Имя Фамилия

            Regex Name = new Regex(@"^[A-Z][a-z]+$"); // Регулярка на Имя без фамилии

            Match match1 = NameAndSurname.Match(NewName);
            Match match2 = Name.Match(NewName);

            if(match1.Success || match2.Success)  //Если какой то из них true то обновляем юзера 
            {
                _authService.User.RealName = NewName;
                _introContext.Update(_authService.User);
                _introContext.SaveChanges();

                return "Name was updated!";
            }

            if(_randomService.Integer % 2 == 0) // Рандомом выкидываем строку для обработки в js
            {
                return "Update Error";
            }
            else
            {
                return "No Errors";
            }

            //return NewName + " changed";
        }

        [HttpPost]
        public JsonResult ChangeLogin([FromBody] string NewLogin)
        {
            string message = "Ok";

            if(_authService.User == null)
            {
                message = "Unauthorized";
            }
            else if(string.IsNullOrEmpty(NewLogin))
            {
                message = "Login could not be empty";
            }
            else if(Regex.IsMatch(NewLogin, @"\s"))
            {
                message = "Login could not contain spaces";
            }
            else if(_introContext.Users.Where(u => u.Login == NewLogin).Count() > 0)
            {
                message = "Login in use";
            }

            if(message == "Ok")
            {
                _authService.User.Login = NewLogin;
                _introContext.SaveChanges();
            }

            return Json(message);
        }

        [HttpPut]
        public JsonResult ChangeEmail([FromForm] string NewEmail)
        {
            string message = "Ok";

            if (_authService.User == null)
            {
                message = "Unauthorized";
            }
            else if (string.IsNullOrEmpty(NewEmail))
            {
                message = "Email could not be empty";
            }
            else if (!Regex.IsMatch(NewEmail, @"^[A-z][A-z\d_]{3,16}@([a-z]{1,10}\.){1,5}[a-z]{2,3}$"))
            {
                message = "Email has wrong format";
            }
            else if (_introContext.Users.Where(u => u.Email == NewEmail).Count() > 0)
            {
                message = "Email in use";
            }

            if (message == "Ok")
            {
                _authService.User.Email = NewEmail;
                _introContext.SaveChanges();
            }

            return Json(message);
        }

        [HttpPost]
        public JsonResult ChangePassword([FromBody] string NewPassword)
        {
            string message = "Password was changed!";
            string pass = _hasher.Hash(NewPassword + _authService.User.PassSalt);

            if (_authService.User == null)
            {
                message = "Unauthorized";
            }
            else if (string.IsNullOrEmpty(NewPassword))              // На всякий случай проверим, но мы проверям в js и не отправляем данные если меньше 3 символов
            {                                                        // Поэтому пустой праоль не должен быть тут
                message = "Password could not be empty";
            }         
            else if(_authService.User.PassHash.Equals(pass))         // Проверяем был ли уже такой пароль. Если ввели такой же то ошибка
            {
                message = "Password is the same!";
            }

            if (message == "Password was changed!")                  // Если нет ошибок меняем пароль
            {
                _authService.User.PassHash = _hasher.Hash(NewPassword + _authService.User.PassSalt);
                _introContext.SaveChanges();
            }

            return Json(message);
        }

        [HttpPost]
        public JsonResult ChangeAvatar(IFormFile userAvatar)
        {
            if(_authService.User == null)
            {
                return Json(new { Status = "Error", Message = "Forbidden" });
            }

            if(userAvatar == null)
            {
                return Json(new { Status = "Error", Message = "No file" });
            }

            string extension = Path.GetExtension(userAvatar.FileName);
            string fileName = userAvatar.FileName.Replace(extension, "-") + Guid.NewGuid() + extension;

            var file = new FileStream("./wwwroot/img/" + fileName, FileMode.Create);
            userAvatar.CopyToAsync(file).ContinueWith(t => file.Dispose());
               
            if(_authService.User.Avatar != null)
            {
                System.IO.File.Delete("./wwwroot/img/" + _authService.User.Avatar);
            }
            
            _authService.User.Avatar = fileName;
            _introContext.SaveChanges();

            return Json(new { Status = "Ok", Message = fileName });
        }
    }
}
