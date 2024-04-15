using LuxuryHotel.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;

namespace LuxuryHotel.Areas.Reception.Controllers
{
    [Authorize]
    public class CheckIn_CheckOutController : Controller
    {
        private dbDataContext db = new dbDataContext();
        // GET: Reception/CheckIn_CheckOut
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public JsonResult GetDateCheckIn(int RoomID)
        {
            try
            {
                var room = db.CHECKINROOMs
                   .Where(r => r.RoomID == RoomID)
                   .OrderByDescending(r => r.CheckInDate)
                   .FirstOrDefault();
                var CheckInDate = room.CheckInDate;
                var rooms = db.ROOMs
                      .Where(r => r.RoomID == RoomID)
                      .SingleOrDefault();
                var roomType = db.ROOMTYPEs
                    .Where(r => r.RoomTypeID == rooms.RoomTypeID)
                    .Select(r => new
                    {
                        PricePerHour = r.PricePerHour,
                        PriceByDay = r.PriceByDay,
                        PriceOverTime = r.PriceOverTime,
                        OverNightPrice = r.OverNightPrice,
                        PriceFirstHour = r.PriceFirstHour,

                    })
                    .ToList();

                return Json(new { code = 200, roomType = roomType ,  CheckInDate = CheckInDate, msg = "Lấy thông tin loại phòng thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { code = 500, msg = "Lấy thông tin loại phòng thất bại: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
       
        [HttpGet]
        public JsonResult GetRoomArea(string area)
        {
            try
            {
                var rooms = db.ROOMs.Where(r => r.Area == area).Select(r => new
                {
                    RoomID = r.RoomID,
                    RoomName = r.RoomName,
                    RoomStatus = r.RoomStatus,
                    RoomTypeID = r.RoomTypeID,
                    Area = r.Area
                }).ToList();

                return Json(new { code = 200, rooms = rooms, area= area, msg = "Lấy danh sách phòng thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { code = 500, msg = "Lấy danh sách phòng thất bại: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult CheckIn(int RoomID)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingRoom = db.ROOMs.SingleOrDefault(r => r.RoomID == RoomID);

                    if (existingRoom != null)
                    {
                        existingRoom.RoomStatus = "Booked";
                        db.SubmitChanges();

                        CHECKINROOM cHECKINROOM = new CHECKINROOM
                        {
                            BookingID = null,
                            CheckInDate = DateTime.Now,
                            ReceptionID = IDRep(),
                            RoomID = RoomID,
                        };
                        db.CHECKINROOMs.InsertOnSubmit(cHECKINROOM);
                        db.SubmitChanges();
                        return Json(new { code = 200, msg = "Nhận phòng thành công!" });
                    }
                    else
                    {
                        return Json(new { code = 400, msg = "Phòng không tồn tại!" });
                    }
                }

                return Json(new { code = 400, msg = "Dữ liệu không hợp lệ!" });
            }
            catch (Exception ex)
            {
                return Json(new { code = 500, msg = ex.Message });
            }
        }
       
        [HttpPost]
        public JsonResult Clear(int RoomID)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingRoom = db.ROOMs.FirstOrDefault(r => r.RoomID == RoomID);
                    if (existingRoom != null)
                    {
                        existingRoom.RoomStatus = "Available";
                        db.SubmitChanges();                 
                        return Json(new { code = 200, msg = "Dọn phòng thành công!" });
                    }
                    else
                    {
                        return Json(new { code = 400, msg = "Phòng không tồn tại!" });
                    }
                }
                return Json(new { code = 400, msg = "Dữ liệu không hợp lệ!" });
            }
            catch (Exception ex)
            {
                return Json(new { code = 500, msg = ex.Message });
            }
        }
        
        [HttpGet]
        public JsonResult GetCheckOut(int RoomID)
        {
            try
            {
                var room = db.CHECKINROOMs
                   .Where(r => r.RoomID == RoomID)
                   .OrderByDescending(r => r.CheckInDate)
                   .FirstOrDefault();
                var CheckInDate = room.CheckInDate;
                var CheckInID = room.CheckinID;
                var services = db.SERVICEREQUESTs
                   .Where(r => (r.CheckinID == room.CheckinID)&&(r.Status=="True"))
                    .Select(r => new
                    {
                        RequestID = r.RequestID,
                        RequestDate = r.RequestDate,
                        CheckinID = r.CheckinID,
                        ServiceID = r.ServiceID,
                        Status = r.Status,
                        Description = r.Description,
                        ReceptionID = r.ReceptionID,

                    })
                   .ToList();

                return Json(new { code = 200,CheckInID=CheckInID, CheckInDate = CheckInDate, services = services, msg = "Lấy danh sách phòng thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { code = 500, msg = "Lấy danh sách phòng thất bại: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult SubmitCheckOut(int CheckInID, DateTime CheckOutDate, int Total)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    
                    // Tạo đối tượng CUSTOMER mới và thiết lập giá trị
                    CHECKOUTROOM room = new CHECKOUTROOM
                    {
                        // Thiết lập giá trị từ tham số
                        CheckinID= CheckInID,
                        CheckoutDate = CheckOutDate,
                        Toltal=Total,

                    };
                    // Thêm loại phòng mới vào database
                    db.CHECKOUTROOMs.InsertOnSubmit(room);
                    db.SubmitChanges();
                    var checkin = db.CHECKINROOMs.FirstOrDefault(r => r.CheckinID == CheckInID);
                    var roomID = checkin.RoomID;
                    var existingRoom = db.ROOMs.SingleOrDefault(r => r.RoomID == roomID);

                    if (existingRoom != null)
                    {
                        existingRoom.RoomStatus = "Soon";
                        db.SubmitChanges();

                        return Json(new { code = 200, msg = "Checkout successfully." });
                    }
                }

                return Json(new { code = 400, msg = "Invalid data. Please check your inputs." });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                return Json(new { code = 500, msg = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        public int IDRep()
        {
            string receptionUsername = User.Identity.Name;

            // Tìm thông tin của người tiếp tân từ cơ sở dữ liệu
            RECEPTION receptionist = db.RECEPTIONs.SingleOrDefault(r => r.User == receptionUsername);
            return receptionist.ReceptionID;
        }

    }
}