﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discussion.Web.Models;
using Discussion.Web.ViewModels;
using Jusfr.Persistent;
using Microsoft.AspNetCore.Mvc;

namespace Discussion.Web.Controllers
{
    public class AccountController: Controller
    {
        private IRepository<User> _userRepository;
        public AccountController(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }
        
        [Route("/signin")]
        public IActionResult Signin([FromQuery]string returnUrl)
        {
            if (HttpContext.IsAuthenticated())
            {
                return RedirectTo(returnUrl);
            }
            return View();
        }


        [HttpPost]
        [Route("/signin")]        
        public async Task<IActionResult> DoSignin([FromForm]SigninUserViewModel viewModel, [FromQuery]string returnUrl)
        {
            if (HttpContext.IsAuthenticated())
            {
                return RedirectTo(returnUrl);
            }
            
            if (!ModelState.IsValid)
            {
                return View("Signin");
            }
            
            var user = new User
            {
                DisplayName = viewModel.UserName,
                UserName = viewModel.UserName
            };
            await this.HttpContext.SigninAsync(user);

            return RedirectTo(returnUrl);
        }

        
        [HttpPost]
        [Route("/signout")] 
        public async Task<IActionResult> DoSignOut()
        {
            var redirectToHome = RedirectTo("/");
            if (!HttpContext.IsAuthenticated())
            {
                return redirectToHome;
            }

            await this.HttpContext.SignoutAsync();
            return redirectToHome;
        }
        
        
        [Route("/register")]  
        public IActionResult Register()
        {
            if (HttpContext.IsAuthenticated())
            {
                return RedirectTo("/");
            }
            
            return View();
        }
        
        
        [HttpPost]
        [Route("/register")]  
        public IActionResult DoRegister(SigninUserViewModel userViewModel)
        {
//            if (!ModelState.IsValid)
//            {
//                return View("Register");
//            }

            var userNameTaken = _userRepository.All.Any(u => u.UserName == userViewModel.UserName);
            if (userNameTaken)
            {
                ModelState.AddModelError("UserName", "用户名已被其他用户占用。");
                return View("Register");
            }
          
            var newUser = new User
            {
                UserName = userViewModel.UserName,
                HashedPassword = userViewModel.Password,
                CreatedAt = DateTime.UtcNow
            };
            _userRepository.Create(newUser);
            return RedirectTo("/");
        }
        
        
        private IActionResult RedirectTo(string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = "/";
            }

            return Redirect(returnUrl);
        }

    }
}