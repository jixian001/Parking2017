using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Parking.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using System.Security.Claims;

namespace Parking.Web.Controllers
{
    [Authorize]   
    public class AccountController : Controller
    {
        private AppUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
            }
        }

        private IAuthenticationManager AuthManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                string message = "";
                var user =await UserManager.FindByNameAsync(model.UserName);
                if (user != null)
                {
                    user = await UserManager.FindAsync(model.UserName, model.Password);
                    if (user != null)
                    {
                        ClaimsIdentity ident = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                        AuthManager.SignOut();
                        AuthManager.SignIn(new AuthenticationProperties
                        {
                            IsPersistent = model.RemeberMe
                        }, ident);
                        return RedirectToAction("Index", "Home");
                    }
                    message = "无效的密码，请输入正确的密码！";
                }
                else
                {
                    message = "当前用户名不存在，请输入正确的用户名！";
                }
                ModelState.AddModelError("", message);
            }
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var oldUser = await UserManager.FindByNameAsync(model.UserName);
                if (oldUser == null)
                {
                    AppUser user = new AppUser { UserName = model.UserName };
                    var result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Login");
                    }
                    AddErrors(result);
                }
                ModelState.AddModelError("", "当前用户名已经存在。");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult LogOff()
        {
            AuthManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index","Home");
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <returns></returns>
        public ActionResult Manage()
        {
            ViewBag.UserName = User.Identity.GetUserName();           
            return View();
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Manage(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                //再绑定下认证信息
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    ClaimsIdentity ident = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                    AuthManager.SignOut();
                    AuthManager.SignIn(new AuthenticationProperties
                    {
                        IsPersistent = false
                    }, ident);
                    return RedirectToAction("Index", "Home");
                }
                return RedirectToAction("Index", "Home");
            }
            AddErrors(result);
            return View(model);
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Delete()
        {           
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                var result = await UserManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    AuthManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.UserName = "删除用户失败";
                    AddErrors(result);                   
                }
            }
            ViewBag.UserName = "删除用户失败";
            return View("Manage");
        }

        [AllowAnonymous]
        public ContentResult Validate()
        {
            var state = HttpContext.User.Identity.Name;
            if (string.IsNullOrEmpty(state))
            {
                return Content("false");
            }
            else
            {
                return Content("true");
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
    }
}