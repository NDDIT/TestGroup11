using LuxuryHotel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LuxuryHotel.Areas.Reception.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        
        private dbDataContext db = new dbDataContext();
        // GET: Reception/Customer
        
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public JsonResult GetCustomerList()
        {
            try
            {
                var customer = db.CUSTOMERs
                    .Select(r => new
                    {
                        CustomerID = r.CustomerID,
                        User = r.User,
                        Password = r.Password,
                        FullName = r.FullName,
                        Email = r.Email,
                        PhoneNumber = r.PhoneNumber,

                    })
                    .ToList();

                return Json(new { code = 200, customer = customer, msg = "Lấy thông tin loại phòng thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { code = 500, msg = "Lấy thông tin loại phòng thất bại: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public JsonResult CreateCustomer(int CustomerID, string User, string Password, string FullName, string Email, string PhoneNumber)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Tạo đối tượng CUSTOMER mới và thiết lập giá trị
                    CUSTOMER customer = new CUSTOMER
                    {
                        // Thiết lập giá trị từ tham số

                        User = User,
                        Password = Password,
                        FullName = FullName,
                        Email = Email,
                        PhoneNumber = PhoneNumber
                    };

                    // Thêm loại phòng mới vào database
                    db.CUSTOMERs.InsertOnSubmit(customer);
                    db.SubmitChanges();

                    return Json(new { code = 200, msg = "Room Type created successfully." });
                }

                return Json(new { code = 400, msg = "Invalid data. Please check your inputs." });
            }
            catch (Exception ex)
            {
                return Json(new { code = 500, msg = ex.Message });
            }
        }


        [HttpPost]
        public JsonResult Edit(int CustomerID, string User, string Password, string FullName, string Email, string PhoneNumber)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingCustomer = db.CUSTOMERs.SingleOrDefault(r => r.CustomerID == CustomerID);

                    if (existingCustomer != null)
                    {
                        existingCustomer.User = User;

                        // Chuyển đổi giá trị từ chuỗi sang kiểu int
                        existingCustomer.Password = Password;
                        existingCustomer.FullName = FullName;
                        existingCustomer.Email = Email;
                        existingCustomer.PhoneNumber = PhoneNumber;

                        db.SubmitChanges();
                        return Json(new { code = 200, msg = "Customer Type updated successfully." });
                    }
                }

                return Json(new { code = 400, msg = "Invalid data. Please check your inputs." });
            }
            catch (Exception ex)
            {
                return Json(new { code = 500, msg = ex.Message });
            }
        }


        [HttpGet]
        public JsonResult GetCustomerTypeDetails(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return Json(new { code = 400, msg = "Giá trị ID không hợp lệ" }, JsonRequestBehavior.AllowGet);
                }

                var customer = db.CUSTOMERs
                    .Where(r => r.CustomerID == id)
                    .Select(r => new
                    {
                        CustomerID = r.CustomerID,
                        User = r.User,
                        Password = r.Password,
                        FullName = r.FullName,
                        Email = r.Email,
                        PhoneNumber = r.PhoneNumber,
                    })
                    .SingleOrDefault();

                if (customer != null)
                {
                    return Json(new { code = 200, customerList = customer, msg = "Lấy thông tin khách hàng thành công" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { code = 404, msg = "Không tìm thấy thông tin khách hàng" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in GetRoomTypeDetails: " + e.Message);
                return Json(new { code = 500, msg = "Lấy thông tin khách hàng thất bại: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult CheckAndDeleteCustomer(int customerID)
        {
            try
            {
                var customer = db.CUSTOMERs.SingleOrDefault(r => r.CustomerID == customerID);

                if (customer != null)
                {
                    db.CUSTOMERs.DeleteOnSubmit(customer);
                    db.SubmitChanges();

                    return Json(new { code = 200, msg = "customer deleted successfully." });
                }
                else
                {
                    return Json(new { code = 404, msg = "Không tìm thấy khách hàng để xóa." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { code = 500, msg = "Đã xảy ra lỗi khi xóa customer: " + ex.Message });
            }
        }
    }
}
    
