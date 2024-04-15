using LuxuryHotel.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LuxuryHotel.Areas.Reception.Controllers
{
    [Authorize]
    public class RoomController : Controller
    {
        private dbDataContext db = new dbDataContext();

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetAreaList()
        {
            try
            {
                var areas = db.ROOMs.Select(r => r.Area).Distinct().ToList();
                return Json(new { code = 200, areas = areas, msg = "Lấy danh sách khu vực thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { code = 500, msg = "Lấy danh sách khu vực thất bại: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetUtilitiesByRoom(int roomID)
        {
            try
            {
                var utilities = (from ru in db.RoomUtilities
                                 join u in db.Utilities on ru.UtilitiesID equals u.UtilitiesID
                                 where ru.RoomID == roomID
                                 select new
                                 {
                                     UtilitiesID = u.UtilitiesID,
                                     UtilitiesName = u.UtilitiesName,
                                     UtilitiesPicture = u.UtilitiesPicture
                                 }).ToList();

                return Json(new { code = 200, utilities = utilities, msg = "Lấy thông tin tiện ích thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { code = 500, msg = "Lấy thông tin tiện ích thất bại: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetRoomsByArea(string area)
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

                return Json(new { code = 200, rooms = rooms, msg = "Lấy danh sách phòng thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { code = 500, msg = "Lấy danh sách phòng thất bại: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetTypeName(int typeID)
        {
            try
            {
                var typeName = db.ROOMTYPEs.Where(rt => rt.RoomTypeID == typeID).Select(rt => rt.TypeName).FirstOrDefault();

                return Json(new { typeName = typeName }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { code = 500, msg = "Lấy TypeName thất bại: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.RoomTypes = new SelectList(db.ROOMTYPEs.ToList().OrderBy(n => n.TypeName), "RoomTypeID", "TypeName");
            ViewBag.Statuses = new SelectList(new List<string> { "Available", "Booked", "Soon" });
            ViewBag.Utilities = db.Utilities.ToList(); // Assume Utilities is a list of Utility objects
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(ROOM room, FormCollection f)
        {
            if (ModelState.IsValid)
            {
                room.RoomName = f["RoomName"];
                room.RoomStatus = f["RoomStatus"];
                room.Area = f["sArea"];
                room.RoomTypeID = int.Parse(f["RoomTypeID"]);

                db.ROOMs.InsertOnSubmit(room);
                db.SubmitChanges();

                // Lấy RoomID sau khi đã được tạo
                var createdRoom = db.ROOMs.OrderByDescending(r => r.RoomID).FirstOrDefault();
                int createdRoomID = createdRoom.RoomID;

                // Process checkboxes for utilities
                var selectedUtilities = f.GetValues("utilities");
                if (selectedUtilities != null)
                {
                    foreach (var utilityId in selectedUtilities)
                    {
                        var roomUtility = new RoomUtility
                        {
                            RoomID = createdRoomID,
                            UtilitiesID = int.Parse(utilityId)
                        };
                        db.RoomUtilities.InsertOnSubmit(roomUtility);
                    }
                }

                db.SubmitChanges();

                // Về lại trang Quản lý sách
                return RedirectToAction("Index");
            }

            // Nếu ModelState không hợp lệ, truyền dữ liệu cho dropdown và checkbox và quay lại view
            ViewBag.RoomTypes = new SelectList(db.ROOMTYPEs.ToList().OrderBy(n => n.TypeName), "RoomTypeID", "TypeName");
            ViewBag.Statuses = new SelectList(new List<string> { "Available", "Booked", "Soon" });
            ViewBag.Utilities = db.Utilities.ToList();
            return View(room);
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            // Lấy thông tin phòng từ id
            var room = db.ROOMs.FirstOrDefault(r => r.RoomID == id);

            if (room == null)
            {
                return HttpNotFound();
            }

            // Lấy danh sách tiện ích
            ViewBag.Utilities = db.Utilities.ToList();
            ViewBag.RoomUtilities = db.RoomUtilities.Where(ru => ru.RoomID == room.RoomID).Select(ru => ru.UtilitiesID).ToList();

            // Lấy danh sách loại phòng
            ViewBag.RoomTypes = new SelectList(db.ROOMTYPEs.ToList().OrderBy(n => n.TypeName), "RoomTypeID", "TypeName");

            // Lấy danh sách trạng thái
            ViewBag.Statuses = new SelectList(new List<string> { "Available", "Booked", "Soon" });
           
            // Truyền dữ liệu phòng và hiển thị form chỉnh sửa
            return View(room);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(ROOM room, FormCollection f)
        {
            if (ModelState.IsValid)
            {
                // Lấy lại thông tin phòng từ cơ sở dữ liệu để có dữ liệu cập nhật đầy đủ
                var existingRoom = db.ROOMs.FirstOrDefault(r => r.RoomID == room.RoomID);

                if (existingRoom == null)
                {
                    return HttpNotFound();
                }

                // Cập nhật thông tin phòng
                existingRoom.RoomName = room.RoomName;
                existingRoom.RoomTypeID = room.RoomTypeID;
                existingRoom.RoomStatus = room.RoomStatus;
                existingRoom.Area = f["sArea"];

                // Xóa các tiện ích cũ của phòng
                var existingUtilities = db.RoomUtilities.Where(ru => ru.RoomID == existingRoom.RoomID);
                db.RoomUtilities.DeleteAllOnSubmit(existingUtilities);

                // Thêm lại các tiện ích mới của phòng
                var selectedUtilities = f.GetValues("utilities");
                if (selectedUtilities != null)
                {
                    foreach (var utilityId in selectedUtilities)
                    {
                        var utilityRoom = new RoomUtility
                        {
                            RoomID = existingRoom.RoomID,
                            UtilitiesID = int.Parse(utilityId)
                        };
                        db.RoomUtilities.InsertOnSubmit(utilityRoom);
                    }
                }

                // Lưu thay đổi vào cơ sở dữ liệu
                db.SubmitChanges();

                // Quay lại trang quản lý phòng
                return RedirectToAction("Index");
            }

            // Nếu ModelState không hợp lệ, truyền dữ liệu cho dropdown và checkbox và quay lại view
            ViewBag.Utilities = db.Utilities.ToList();
            ViewBag.RoomTypes = new SelectList(db.ROOMTYPEs.ToList().OrderBy(n => n.TypeName), "RoomTypeID", "TypeName");
            ViewBag.Statuses = new SelectList(new List<string> { "Available", "Booked", "Soon" });

            return View(room);
        }

        [HttpPost]
        public JsonResult DeleteRoom(int roomID)
        {
            try
            {
                // Tìm phòng cần xóa từ database
                ROOM roomToDelete = db.ROOMs.SingleOrDefault(r => r.RoomID == roomID);

                if (roomToDelete == null)
                {
                    return Json(new { code = 404, msg = "Không tìm thấy phòng để xóa." });
                }

                // Xóa các bản ghi trong bảng RoomUtility
                var roomUtilitiesToDelete = db.RoomUtilities.Where(ru => ru.RoomID == roomID);
                db.RoomUtilities.DeleteAllOnSubmit(roomUtilitiesToDelete);

                // Xóa các bản ghi trong bảng Image
                var roomImagesToDelete = db.Images.Where(i => i.RoomID == roomID);
                db.Images.DeleteAllOnSubmit(roomImagesToDelete);

                // Xóa các bản ghi trong bảng Review
                var roomReviewsToDelete = db.REVIEWs.Where(r => r.RoomID == roomID);
                db.REVIEWs.DeleteAllOnSubmit(roomReviewsToDelete);

                // Xóa phòng
                db.ROOMs.DeleteOnSubmit(roomToDelete);

                db.SubmitChanges();

                return Json(new { code = 200, msg = "Xóa phòng thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { code = 500, msg = "Lỗi khi xóa phòng: " + ex.Message });
            }
        }
       

    }
}
