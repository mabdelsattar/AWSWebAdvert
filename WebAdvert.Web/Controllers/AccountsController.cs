using System;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAdvert.Web.Models.Accounts;
using Amazon.AspNetCore.Identity.Cognito;

namespace WebAdvert.Web.Controllers
{
    public class AccountsController : Controller
    {
        private readonly SignInManager<CognitoUser> _signInManager;
        private readonly UserManager<CognitoUser> _userManager;
        private readonly CognitoUserPool _pool;

        public AccountsController(SignInManager<CognitoUser> signInManager, UserManager<CognitoUser> userManager, CognitoUserPool pool)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _pool = pool;
        }
        public async Task<IActionResult> SignUp() 
        {
            var model = new SignupModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignupModel model)
        {
            //we need to inject sign in manager,  cognito pool
            //for now let's write everthing here ,then will refactor it and move it to business
            if (ModelState.IsValid)
            {
                var user = _pool.GetUser(model.Email);
                //user is alreay exists
                if (user.Status != null)
                { 
                    ModelState.AddModelError("UserExists", "user is alreay exists");
                    return View(model);
                }
                //now we need to add extra fields we made like 'name' ,'birthdate,'ect'
                user.Attributes.Add(CognitoAttribute.Name.AttributeName, "AbdelSattar");
                user.Attributes.Add(CognitoAttribute.Gender.AttributeName, "Male");
                user.Attributes.Add(CognitoAttribute.Address.AttributeName, "6 Wehdah,Cairo");
                user.Attributes.Add(CognitoAttribute.BirthDate.AttributeName, "14-12-1992");

                var createdUser = await _userManager.CreateAsync(user, model.Password);
                if (createdUser.Succeeded)
                {
                    return RedirectToAction("Confirm");
                }
            
            }
            return View();
        }

        public async Task<IActionResult> Confirm(ConfirmModel model)
        {
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Confirm_Post(ConfirmModel model)
        {
            //need to fetch user first 
           
                    
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                //user is alreay exists
                if (user == null)
                {
                    ModelState.AddModelError("NotFound", "user is not exists");
                    return View(model);
                }

                var result = await (_userManager as CognitoUserManager<CognitoUser>).ConfirmSignUpAsync(user, model.Code,true);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError(item.Code, item.Description);
                    }
                    return View(model);
                }

            }
            return View();
        }


        public IActionResult Index()
        {
            return View();
        }
    }
}