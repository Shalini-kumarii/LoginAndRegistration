using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Login_Registration.Models;
using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Http;  //This is where session comes from
namespace Login_Registration.Controllers     //be sure to use your own project's namespace!
{
    public class UserController : Controller   //remember inheritance??
    {
        private MyContext _context;

        // here we can "inject" our context service into the constructor
        public UserController(MyContext context)
        {
            _context = context;
        }
        [HttpGet("")]      // Both lines can be written in one line
        public ViewResult Index()
        {



            return View("Index");
        }
        [HttpPost("register")]
        public IActionResult Register(User newuser)
        {
            // Check initial ModelState
            if (ModelState.IsValid)
            {
                // If a User exists with provided email
                if (_context.Users.Any(u => u.Email == newuser.Email))
                {
                    // Manually add a ModelState error to the Email field, with provided
                    // error message
                    ModelState.AddModelError("Email", "Email already in use!");

                    // You may consider returning to the View at this point
                    return View("Index");
                }
                else
                {
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    newuser.Password = Hasher.HashPassword(newuser, newuser.Password);
                    _context.Add(newuser);
                    _context.SaveChanges();
                    return RedirectToAction("LoginPageRander");


                }
            }
            else
            {
                return View("Index");
            }

        }


        [HttpGet("LoginPageRander")]

        public ViewResult LoginPageRander()
        {
            return View("Login");
        }

        [HttpPost("Login")]
        public IActionResult Login(LoginUser userSubmission)
        {
            if (ModelState.IsValid)
            {
                // If inital ModelState is valid, query for a user with provided email
                var userInDb = _context.Users.FirstOrDefault(u => u.Email == userSubmission.Email);
                // If no user exists with provided email
                if (userInDb == null)
                {
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Login");
                }

                // Initialize hasher object
                var hasher = new PasswordHasher<LoginUser>();

                // verify provided password against hash stored in db
                var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);

                // result can be compared to 0 for failure
                if (result == 0)
                {
                    // handle failure (this should be similar to how "existing email" is handled)
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Login");
                }

                else
                {
                    System.Console.WriteLine("Login Passed");
                    HttpContext.Session.SetInt32("UserId", userInDb.UserId);
                    return RedirectToAction("Success");
                }
            }
            else
            {
                ModelState.AddModelError("Email", "Invalid Email/Password");
                return View("Login");
            }

        }

        [HttpGet("Success")]
        public IActionResult Success()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return View("Index");
            }
            else
            {
                int? UserId = HttpContext.Session.GetInt32("UserId");
                User LoggedInUser = _context.Users.FirstOrDefault(user => user.UserId == UserId);

                return View("Success", LoggedInUser);
            }
        }

        [HttpGet("Logout")]

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }



    }

}







