using LuxuryHotel.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LuxuryHotel.Areas.Reception.Controllers
{
    [Authorize]
    public class PriceFormationController : Controller
    {
        private dbDataContext _db = new dbDataContext();
        
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetRoomPrice()
        {
            try
            {
                var roomTypes = _db.ROOMTYPEs
                    .Select(r => new
                    {
                        RoomTypeID = r.RoomTypeID,
                        TypeName = r.TypeName,
                        PricePerHour = r.PricePerHour,
                        PriceByDay = r.PriceByDay,
                        OverNightPrice = r.OverNightPrice,
                        PriceFirstHour = r.PriceFirstHour,
                        PriceOverTime = r.PriceOverTime
                    })
                    .ToList();

                return Json(new { code = 200, roomTypes = roomTypes, msg = "Lấy thông tin loại phòng thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { code = 500, msg = "Lấy thông tin loại phòng thất bại: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public JsonResult CreateRoomType(int RoomTypeID, string TypeName, int PricePerHour, int PriceByDay, int OverNightPrice, int PriceFirstHour, int PriceOverTime)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Tạo đối tượng ROOMTYPE mới và thiết lập giá trị
                    ROOMTYPE roomType = new ROOMTYPE
                    {
                        // Thiết lập giá trị từ tham số
                
                        TypeName = TypeName,
                        PricePerHour = PricePerHour,
                        PriceByDay = PriceByDay,
                        OverNightPrice = OverNightPrice,
                        PriceFirstHour = PriceFirstHour,
                        PriceOverTime = PriceOverTime
                    };

                    // Thêm loại phòng mới vào database
                    _db.ROOMTYPEs.InsertOnSubmit(roomType);
                    _db.SubmitChanges();

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
        public JsonResult Edit(int RoomTypeID, string TypeName, int PricePerHour, int PriceByDay, int OverNightPrice, int PriceFirstHour, int PriceOverTime)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingRoomType = _db.ROOMTYPEs.SingleOrDefault(r => r.RoomTypeID == RoomTypeID);

                    if (existingRoomType != null)
                    {
                        existingRoomType.TypeName = TypeName;

                        // Chuyển đổi giá trị từ chuỗi sang kiểu int
                        existingRoomType.PricePerHour = PricePerHour;
                        existingRoomType.PriceByDay = PriceByDay;
                        existingRoomType.OverNightPrice = OverNightPrice;
                        existingRoomType.PriceFirstHour = PriceFirstHour;
                        existingRoomType.PriceOverTime = PriceOverTime;

                        _db.SubmitChanges();
                        return Json(new { code = 200, msg = "Room Type updated successfully." });
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
        public JsonResult GetRoomTypeDetails(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return Json(new { code = 400, msg = "Giá trị ID không hợp lệ" }, JsonRequestBehavior.AllowGet);
                }

                var roomType = _db.ROOMTYPEs
                    .Where(r => r.RoomTypeID == id)
                    .Select(r => new
                    {
                        RoomTypeID = r.RoomTypeID,
                        TypeName = r.TypeName,
                        PricePerHour = r.PricePerHour,
                        PriceByDay = r.PriceByDay,
                        OverNightPrice = r.OverNightPrice,
                        PriceFirstHour = r.PriceFirstHour,
                        PriceOverTime = r.PriceOverTime
                    })
                    .SingleOrDefault();

                if (roomType != null)
                {
                    return Json(new { code = 200, roomType = roomType, msg = "Lấy thông tin loại phòng thành công" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { code = 404, msg = "Không tìm thấy thông tin loại phòng" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in GetRoomTypeDetails: " + e.Message);
                return Json(new { code = 500, msg = "Lấy thông tin loại phòng thất bại: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult CheckAndDeleteRoomType(int roomTypeId)
        {
            try
            {
                var roomType = _db.ROOMTYPEs.SingleOrDefault(r => r.RoomTypeID == roomTypeId);

                if (roomType != null)
                {
                    // Kiểm tra xem có phòng đang sử dụng loại phòng này hay không
                    var roomsUsingRoomType = _db.ROOMs.Where(r => r.RoomTypeID == roomTypeId).ToList();

                    if (roomsUsingRoomType.Count > 0)
                    {
                        // Có phòng đang sử dụng, không xóa
                        return Json(new { code = 400, msg = "Tồn tại phòng đang sử dụng loại phòng này. Không thể xóa." });
                    }

                    // Không có phòng đang sử dụng, xóa RoomType
                    _db.ROOMTYPEs.DeleteOnSubmit(roomType);
                    _db.SubmitChanges();

                    return Json(new { code = 200, msg = "Room Type deleted successfully." });
                }
                else
                {
                    return Json(new { code = 404, msg = "Không tìm thấy loại phòng để xóa." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { code = 500, msg = "Đã xảy ra lỗi khi xóa Room Type: " + ex.Message });
            }
        }


    }
}
