﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;



namespace MyProject.Controllers
{
    public class AuthenticationController : Controller
    {
        
        public IActionResult Index()
        {
            return View();
        }
    }
}

