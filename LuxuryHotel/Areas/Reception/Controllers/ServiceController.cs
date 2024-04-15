using LuxuryHotel.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace LuxuryHotel.Areas.Reception.Controllers
{
    [Authorize]
    public class ServiceController : Controller
    {
        private dbDataContext db = new dbDataContext();
        // GET: Reception/Service
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public JsonResult GetService()
        {
            try
            {
                var services = db.SERVICEs
                    .Select(r => new
                    {
                        ServiceID = r.ServiceID,
                        ServiceName = r.ServiceName,
                        ServicePrice = r.ServicePrice,

                    })
                    .ToList();

                return Json(new { code = 200, services = services, msg = "Lấy thông tin dịch vụ thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { code = 500, msg = "Lấy thông tin dịch vụ thất bại: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult Details(int ServiceID)
        {
            try
            {
                var sv = (from s in db.SERVICEs where( s.ServiceID == ServiceID)
     
                          select new
                          {
                              ServiceName = s.ServiceName,
                              ServicePrice = s.ServicePrice
                          }).SingleOrDefault();
                if (sv != null)
                {
                    return Json(new { code = 200, sv = sv, msg = "Lấy thông tin dịch vu thành công" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { code = 404, msg = "Dịch vụ không tồn tại" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { code = 500, msg = "Lấy thông tin dịch vụ thất bại" + e.Message });
            }
        }
        [HttpPost]
        public JsonResult DeleteService(int serviceId)
        {
            try
            {
                var service = db.SERVICEs.SingleOrDefault(s => s.ServiceID == serviceId);

                if (service != null)
                {
                    // Kiểm tra xem có service request đang sử dụng dịch vụ này hay không
                    var servicesInRequests = db.SERVICEREQUESTs.Where(sr => sr.ServiceID == serviceId).ToList();

                    if (servicesInRequests.Count >= 1 )
                    {
                        // Có service request đang sử dụng, không xóa
                        return Json(new { code = 400, servicesInRequests= servicesInRequests, msg = "Tồn tại service request đang sử dụng dịch vụ này. Không thể xóa." });
                    }

                    // Không có service request đang sử dụng, xóa dịch vụ
                    db.SERVICEs.DeleteOnSubmit(service);
                    db.SubmitChanges();

                    return Json(new { code = 200, msg = "Service deleted successfully." });
                }
                else
                {
                    return Json(new { code = 404, msg = "Không tìm thấy dịch vụ để xóa." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { code = 500, msg = "Đã xảy ra lỗi khi xóa dịch vụ: " + ex.Message });
            }
        }






        [HttpPost]
        public JsonResult SaveService(int ServiceID, string ServiceName, int ServicePrice)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingService = db.SERVICEs.SingleOrDefault(s => s.ServiceID == ServiceID);

                    if (existingService != null)
                    {
                        // Cập nhật thông tin của dịch vụ
                        existingService.ServiceName = ServiceName;
                        existingService.ServicePrice = ServicePrice;

                        db.SubmitChanges();

                        return Json(new { code = 200, msg = "Dịch vụ được cập nhật thành công." });
                    }
                    else
                    {
                        // Tạo dịch vụ mới nếu không tìm thấy dịch vụ có sẵn
                        SERVICE newService = new SERVICE
                        {
                            ServiceName = ServiceName,
                            ServicePrice = ServicePrice
                        };

                        db.SERVICEs.InsertOnSubmit(newService);
                        db.SubmitChanges();

                        return Json(new { code = 200, msg = "Dịch vụ được tạo mới thành công." });
                    }
                }

                return Json(new { code = 400, msg = "Dữ liệu không hợp lệ. Vui lòng kiểm tra đầu vào của bạn." });
            }
            catch (Exception ex)
            {
                return Json(new { code = 500, msg = "Đã xảy ra lỗi khi lưu dịch vụ: " + ex.Message });
            }
        }



    }
}