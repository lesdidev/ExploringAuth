﻿using Microsoft.AspNetCore.Mvc;
using IdentityServer.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Security.Claims;
using IdentityServer4.Services;

namespace IdentityServer.Controllers
{
    public class AuthController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IIdentityServerInteractionService _identityServerInteractionService;

        public AuthController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IIdentityServerInteractionService identityServerInteractionService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _identityServerInteractionService = identityServerInteractionService;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            return View(new LoginVM() { ReturnUrl = returnUrl });
        }

        [HttpGet]
        public IActionResult Register(string returnUrl)
        {
            return View(new RegisterVM() { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
            {
                return View(loginVM);
            }

            var loginResult = await _signInManager.PasswordSignInAsync(loginVM.Username, loginVM.Password, false, false);

            if (!loginResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Wrong Username or Password");
                return View(loginVM);
            }

            return Redirect(loginVM.ReturnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
            {
                return View(registerVM);
            }

            var user = new IdentityUser(registerVM.Username);
            var registerResult = await _userManager.CreateAsync(user, registerVM.Password);

            if (!registerResult.Succeeded)
            {
                foreach(var err in registerResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                    return View(registerVM);
                }
            }

            await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Name, registerVM.Fullname));
            await _signInManager.SignInAsync(user, false);

            return Redirect(registerVM.ReturnUrl);
        }

        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            await _signInManager.SignOutAsync();

            var logoutRequest = await _identityServerInteractionService.GetLogoutContextAsync(logoutId);

            if (string.IsNullOrEmpty(logoutRequest?.PostLogoutRedirectUri))
            {
                return LocalRedirect("/");
            }

            return Redirect(logoutRequest.PostLogoutRedirectUri);
        }
    }
}
