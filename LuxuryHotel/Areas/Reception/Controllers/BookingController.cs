using LuxuryHotel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LuxuryHotel.Areas.Reception.Controllers
{
    public class BookingController : Controller
    {
        private dbDataContext _db = new dbDataContext();
        // GET: Reception/Booking
        public ActionResult Index()
        {
            ViewBag.RoomNames = new SelectList(_db.ROOMs.ToList().OrderBy(n => n.RoomName), "RoomID", "RoomName");
            ViewBag.Statuses = new SelectList(new List<string> { "Paid", "UnPaid", "Paid and Checked in", "UnPaid and Checked in" });
            ViewBag.CustomerNames = new SelectList(_db.CUSTOMERs.ToList().OrderBy(n => n.CustomerID), "CustomerID", "FullName");

            return View();
        }
        [HttpGet]
        public JsonResult GetBookList()
        {
            try
            {
                var booking = _db.BOOKINGs
                    .Select(r => new
                    {
                        BookingID = r.BookingID,
                        BookingDate = r.BookingDate,
                        CheckInDate = r.CheckInDate,
                        CheckOutDate = r.CheckOutDate,
                        RoomID = r.RoomID,
                        PaymentStatus = r.PaymentStatus,
                        CustomerID = r.CustomerID
                    })
                    .ToList();

                var bookingWithDetails = booking.Select(b => new
                {
                    BookingID = b.BookingID,
                    BookingDate = b.BookingDate,
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    RoomID = b.RoomID,
                    PaymentStatus = b.PaymentStatus,
                    CustomerID = b.CustomerID,
                    TypeName = GetRoomTypeName(b.RoomID ?? 0),
                    FullName = GetCustomerFullName(b.CustomerID ?? 0)
                });

                return Json(new { code = 200, booking = bookingWithDetails, msg = "Lấy thông tin đặt phòng thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { code = 500, msg = "Lấy thông tin đặt phòng thất bại: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public string GetRoomTypeName(int? roomID)
        {
            try
            {
                if (roomID.HasValue)
                {
                    var roomType = _db.ROOMs
                        .Where(r => r.RoomID == roomID.Value)
                        .Select(r => r.RoomName)
                        .SingleOrDefault();

                    return roomType ?? "Unknown";
                }
                else
                {
                    return "Unknown";
                }
            }
            catch (Exception e)
            {
                return "Error: " + e.Message;
            }
        }

        public string GetCustomerFullName(int? customerID)
        {
            try
            {
                if (customerID.HasValue)
                {
                    var customer = _db.CUSTOMERs
                        .Where(c => c.CustomerID == customerID.Value)
                        .Select(c => c.FullName)
                        .SingleOrDefault();

                    return customer ?? "Unknown";
                }
                else
                {
                    return "Unknown";
                }
            }
            catch (Exception e)
            {
                return "Error: " + e.Message;
            }
        }



        [HttpPost]
        public JsonResult CreateBooking(int BookingID, DateTime BookingDate, DateTime CheckInDate, DateTime CheckOutDate, int RoomID, string PaymentStatus, int CustomerID)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Tạo đối tượng ROOMTYPE mới và thiết lập giá trị
                    BOOKING booking = new BOOKING
                    {
                        // Thiết lập giá trị từ tham số

                        BookingDate = BookingDate,
                        CheckInDate = CheckInDate,
                        CheckOutDate = CheckOutDate,
                        RoomID = RoomID,
                        PaymentStatus = PaymentStatus,
                        CustomerID = CustomerID
                    };

                    // Thêm loại phòng mới vào database
                    _db.BOOKINGs.InsertOnSubmit(booking);
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
        public JsonResult Edit(int BookingID, DateTime BookingDate, DateTime CheckInDate, DateTime CheckOutDate, int RoomID, string PaymentStatus, int CustomerID)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingbooking = _db.BOOKINGs.SingleOrDefault(r => r.BookingID == BookingID);

                    if (existingbooking != null)
                    {
                        existingbooking.BookingDate = BookingDate;

                        // Chuyển đổi giá trị từ chuỗi sang kiểu int
                        existingbooking.CheckInDate = CheckInDate;
                        existingbooking.CheckOutDate = CheckOutDate;
                        existingbooking.RoomID = RoomID;
                        existingbooking.PaymentStatus = PaymentStatus;
                        existingbooking.CustomerID = CustomerID;

                        _db.SubmitChanges();
                        return Json(new { code = 200, msg = "Booking updated successfully." });
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
        public JsonResult GetBookingDetails(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return Json(new { code = 400, msg = "Giá trị ID không hợp lệ" }, JsonRequestBehavior.AllowGet);
                }

                var booking = _db.BOOKINGs
                    .Where(r => r.BookingID == id)
                    .Select(r => new
                    {
                        BookingID = r.BookingID,
                        BookingDate = r.BookingDate,
                        CheckInDate = r.CheckInDate,
                        CheckOutDate = r.CheckOutDate,
                        RoomID = r.RoomID,
                        PaymentStatus = r.PaymentStatus,
                        CustomerID = r.CustomerID
                    })
                    .SingleOrDefault();

                if (booking != null)
                {
                    return Json(new { code = 200, booking = booking, msg = "Lấy thông tin đặt phòng thành công" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { code = 404, msg = "Không tìm thấy thông tin đặt phòng" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error in GetBookingTypeDetails: " + e.Message);
                return Json(new { code = 500, msg = "Lấy thông tin đặt phòng thất bại: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult CheckAndDeleteBooking(int BookingID)
        {
            try
            {
                var booking = _db.BOOKINGs.SingleOrDefault(r => r.BookingID == BookingID);

                if (booking != null)
                {
                    _db.BOOKINGs.DeleteOnSubmit(booking);
                    _db.SubmitChanges();
                    return Json(new { code = 200, msg = "Booking deleted successfully." });
                }
                else
                {
                    return Json(new { code = 404, msg = "Không tìm thấy đặt phòng để xóa." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { code = 500, msg = "Đã xảy ra lỗi khi xóa Booking: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult CheckIn(int BookingID)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var booking = _db.BOOKINGs.SingleOrDefault(r => r.BookingID == BookingID);
                    var existingRoom = _db.ROOMs.SingleOrDefault(r => r.RoomID == booking.RoomID);

                    if (existingRoom != null&& existingRoom.RoomStatus== "Available")
                    {
                        existingRoom.RoomStatus = "Booked";
                        _db.SubmitChanges();

                        CHECKINROOM cHECKINROOM = new CHECKINROOM
                        {
                            BookingID = BookingID,
                            CheckInDate = DateTime.Now,
                            ReceptionID = IDRep(),
                            RoomID = booking.RoomID,
                        };
                        _db.CHECKINROOMs.InsertOnSubmit(cHECKINROOM);
                        _db.SubmitChanges();
                        if (booking.PaymentStatus == "Paid")
                        {
                            booking.PaymentStatus = "Paid and Checkin";
                            _db.SubmitChanges();
                        }
                        else
                        {
                            booking.PaymentStatus = "Paid and Checkin";
                            _db.SubmitChanges();
                        }
                            return Json(new { code = 200, msg = "Nhận phòng thành công!" });
                    }
                    else
                    {
                        return Json(new { code = 400, msg = "Chọn phòng khác!" });
                    }
                }

                return Json(new { code = 400, msg = "Dữ liệu không hợp lệ!" });
            }
            catch (Exception ex)
            {
                return Json(new { code = 500, msg = ex.Message });
            }
        }
        public int IDRep()
        {
            string receptionUsername = User.Identity.Name;

            // Tìm thông tin của người tiếp tân từ cơ sở dữ liệu
            RECEPTION receptionist = _db.RECEPTIONs.SingleOrDefault(r => r.User == receptionUsername);
            return receptionist.ReceptionID;
        }
    }
}