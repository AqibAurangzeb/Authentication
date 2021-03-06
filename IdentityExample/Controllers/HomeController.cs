using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NETCore.MailKit.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityExample.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager; // create, delete user info - manage user info
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailService _emailService;

        public HomeController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmailService emailService)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(signInManager));

        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize] // Are you allowed to come here? 
        public IActionResult Secret()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // login functionallity
            var user = await _userManager.FindByNameAsync(username);

            if (user != null)
            {
                // sign in
                var result = await _signInManager.PasswordSignInAsync(user, password, false, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }

            }

            throw new Exception("login_attempt_failed");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string password)
        {
            // register functionallity
            var user = new IdentityUser
            {
                UserName = username,
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // generation of email token
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var link = Url.Action(nameof(VerifyEmail), "Home", new { userId = user.Id, code }, Request.Scheme, Request.Host.ToString());

                await _emailService.SendAsync("test@test.com", "Email Verify", $"<a href=\"{link}\">VerifyEmail Email<a>", true);

                return RedirectToAction("EmailVerification");

                //// sign in
                //var signInResult = await _signInManager.PasswordSignInAsync(user, password, false, false);

                //if (signInResult.Succeeded)
                //{
                //    return RedirectToAction("Index");
                //}



            }

            throw new Exception("register_attempt_failed");
        }

        public async Task<IActionResult> VerifyEmail(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return BadRequest();

            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
                return View();

            return BadRequest();
        }

        public IActionResult EmailVerification() => View();
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index");
        }
    }
}
