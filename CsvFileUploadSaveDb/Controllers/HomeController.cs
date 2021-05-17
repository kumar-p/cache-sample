using CsvFileUploadSaveDb.Helpers;
using CsvFileUploadSaveDb.Models;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CsvFileUploadSaveDb.Controllers
{
    public class HomeController : Controller
    {
        private const string PROFILE_KEY = "profile";
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly ICacheHelper _cache;

        public HomeController(
            ILogger<HomeController> logger,
            IWebHostEnvironment env,
            ICacheHelper cache)
        {
            _logger = logger;
            _env = env;
            _cache = cache;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Upload(IFormFile file)
        {

            string fileName = $"{_env.WebRootPath}\\{file.FileName}";

            using (FileStream fs = System.IO.File.Create(fileName))
            {
                file.CopyTo(fs);
                fs.Flush();
            }

            ViewData["message"] = $"{file.Length} bytes uploaded successfully!";

            UserProfile userProfile = _cache.GetValue<UserProfile>(PROFILE_KEY);

            if (userProfile == null)
            {
                // fetch from DB or some service
                userProfile = new UserProfile
                {
                    UserName = "User1",
                    LastUpdated = DateTime.Now
                };

                // store in cache
                _cache.SetValue<UserProfile>(PROFILE_KEY, userProfile);
            }

            ViewData["userProfile"] = userProfile;

            return View("FileProcessStatus");
        }
    }
}
