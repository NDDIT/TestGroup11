using LuxuryHotel.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using System.Text;
using System.Web.Security;
using System.Security.Cryptography;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
namespace LuxuryHotel.Areas.Reception.Controllers
{
    public class LoginLogoutController : Controller
    {
        private dbDataContext db = new dbDataContext();


        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(FormCollection collection)
        {
            var user = collection["username"];
            var password = collection["pass"];


            // Kiểm tra và xử lý lỗi
            if (string.IsNullOrEmpty(user))
            {
                ViewData["Loi1"] = "Phải nhập tên đăng nhập";
            }
            else if (string.IsNullOrEmpty(password))
            {
                ViewData["Loi2"] = "Phải nhập mật khẩu";
            }
            else
            {
                // Gán giá trị cho tài khoản khách hàng (kh) nếu tìm thấy tài khoản
                RECEPTION ad = db.RECEPTIONs.SingleOrDefault(n => n.User == user && n.Password == password);

                if (ad != null)
                {
                    ViewBag.Thongbao = "Chúc mừng đăng nhập thành công";

                    FormsAuthentication.SetAuthCookie(ad.User, false);

                    return RedirectToAction("Index", "Statistic");
                }
                else
                {
                    ViewBag.Thongbao = "Tên đăng nhập hoặc mật khẩu không đúng";
                }
            }
            return View();
        }
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
    }
}