using LuxuryHotel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LuxuryHotel.Controllers
{
    public class UserController : Controller
    {

        dbDataContext db = new dbDataContext();
        // GET: User
       
        [HttpGet]
        public ActionResult DangXuat()
        {
            // Xóa phiên đăng nhập
            Session["User"] = null;

            // Xóa cookies (nếu có)
            if (Request.Cookies["User"] != null)
            {
                Response.Cookies["User"].Expires = DateTime.Now.AddDays(-1);
            }

            if (Request.Cookies["Password"] != null)
            {
                Response.Cookies["Password"].Expires = DateTime.Now.AddDays(-1);
            }

            return RedirectToAction("DangNhap", "User");
        }

        [HttpGet]
        public ActionResult DangNhap()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DangNhap(FormCollection f)
        {
            var url = f["url"];
            if (string.IsNullOrEmpty(url))
                url = "~/LuxuryHotel/Index";
            var sUser = f["User"];
            var sPassword = f["Password"];

            if (String.IsNullOrEmpty(sUser))
            {
                ViewData["Err1"] = "Bạn chưa nhập tên đăng nhập";
                return View();
            }
            else if (String.IsNullOrEmpty(sPassword))
            {
                ViewData["Err2"] = "Phải nhập mật khẩu";
                return View();
            }
            else
            {
                CUSTOMER cs = db.CUSTOMERs.SingleOrDefault(n => n.User == sUser && n.Password == sPassword);
                if (cs != null)
                {
                    // Đăng nhập thành công
                    Session["User"] = cs;

                    // Kiểm tra và xử lý việc ghi nhớ đăng nhập
                    if (f["remember"] == "true")
                    {
                        HttpCookie cookieUser = new HttpCookie("User");
                        cookieUser.Value = sUser;
                        cookieUser.Expires = DateTime.Now.AddDays(1);
                        Response.Cookies.Add(cookieUser);

                        HttpCookie cookiePassword = new HttpCookie("Password");
                        cookiePassword.Value = sPassword;
                        cookiePassword.Expires = DateTime.Now.AddDays(1);
                        Response.Cookies.Add(cookiePassword);
                    }
                    else
                    {
                        HttpCookie cookieUser = new HttpCookie("User");
                        cookieUser.Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies.Add(cookieUser);

                        HttpCookie cookiePassword = new HttpCookie("Password");
                        cookiePassword.Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies.Add(cookiePassword);
                    }

                    // Chuyển hướng người dùng đến trang chủ
                    return RedirectToAction("Index", "LuxuryHotel");
                }
                else
                {
                    ViewBag.ThongBao = "Tên đăng nhập hoặc mật khẩu không đúng";
                    return View();
                }
            }
        }

        [HttpGet]
        public ActionResult DangKy()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DangKy(FormCollection f, CUSTOMER kh)
        {


            var sUser = f["User"];
            var sPassword = f["Password"];
            var sFullName = f["FullName"];
            var sConfirmPass = f["ConfirmPass"];
            var sEmail = f["Email"];
            var sPhoneNumber = f["PhoneNumber"];

            if (db.CUSTOMERs.SingleOrDefault(n => n.User == sUser) != null)
            {
                ViewBag.ThongBao = "Tên đăng nhập đã tồn tại";
            }
            else if (db.CUSTOMERs.SingleOrDefault(n => n.Email == sEmail) != null)
            {
                ViewBag.ThongBao = "Email đã được sử dụng";

            }
            else if (ModelState.IsValid)
            {
                kh.User = sUser;
                kh.Password = sPassword;
                
                kh.Email = sEmail;
                kh.PhoneNumber = sPhoneNumber;
                db.CUSTOMERs.InsertOnSubmit(kh);
                db.SubmitChanges();
                return RedirectToAction("DangNhap");
            }
            return View("DangKy");
        }
    }
}