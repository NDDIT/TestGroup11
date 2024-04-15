using LuxuryHotel.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LuxuryHotel.Areas.Reception.Controllers
{
    [Authorize]
    public class ImageController : Controller
    {
        // GET: Reception/Image
        private dbDataContext db = new dbDataContext();

        public ActionResult Index()
        {
            return View();
        }
        
        public JsonResult GetRoomImages()
        {
            try
            {
                var rooms = db.ROOMs
                    .Select(r => new
                    {
                       RoomID= r.RoomID,
                       RoomName= r.RoomName,

                    })
                    .ToList();
                return Json(new { code = 200, rooms = rooms, msg = "Lấy danh sách phòng thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { code = 500, msg = "Lấy danh sách phòng thất bại: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        
        public JsonResult GetUrlImagesByRoom(int roomID)
        {
            try
            {
                var images = (from ru in db.Images
                                 where ru.RoomID == roomID
                                 orderby (ru.OderID)
                                 select new
                                 {
                                     ImagePath = ru.ImagePath,
                                     
                                 }).ToList();

                return Json(new { code = 200, images = images, msg = "Lấy thông tin ImagePath thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { code = 500, msg = "Lấy thông tin ImagePath thất bại: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult Edit(int roomID)
        {
            ViewBag.roomID = roomID;
           
            return View();
        }
        [HttpGet]
        public ActionResult GetImages(int roomID)
        {
            try
            {
                var images = (from ru in db.Images
                              where ru.RoomID == roomID
                              orderby (ru.OderID)
                              select new
                              {
                                  ImageID=ru.ImageID,
                                  ImagePath = ru.ImagePath,
                                  OderID=ru.OderID,

                              }).ToList();

                return Json(new { code = 200, images = images, msg = "Lấy thông tin ImagePath thành công" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { code = 500, msg = "Lấy thông tin ImagePath thất bại: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(FormCollection f, HttpPostedFileBase fFileUpload)
        {
            try
            {
                // Kiểm tra nếu file và RoomID hợp lệ
                if ( ModelState.IsValid)
                {
                    // Lấy tên file
                    var sFileName = Path.GetFileName(fFileUpload.FileName);

                    // Lấy đường dẫn lưu file
                    var path = Path.Combine(Server.MapPath("/Admin/Images/Room"), sFileName);

                    // Kiểm tra ảnh đã tồn tại chưa để lưu lên thư mục
                    if (!System.IO.File.Exists(path))
                    {
                        fFileUpload.SaveAs(path);
                    }

                    // Tạo mới đối tượng Image
                    LuxuryHotel.Models.Image images = new LuxuryHotel.Models.Image
                    {
                        RoomID = int.Parse(f["roomID"]),
                        ImagePath = sFileName,
                        OderID = int.Parse(f["iOderID"]),
                    };

                    // Thêm vào CSDL
                    db.Images.InsertOnSubmit(images);
                    db.SubmitChanges();

                    return Json(new { code = 200, msg = "Tạo mới hình ảnh thành công" });
                }

                return Json(new { code = 404, msg = "Tạo mới hình ảnh thất bại. Vui lòng kiểm tra lại dữ liệu đầu vào." });
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine("Error: " + e.Message);
                // Xử lý lỗi và trả về thông báo lỗi
                return Json(new { code = 500, msg = "Lỗi khi tạo mới hình ảnh: " + e.Message });
            }
        }

        [HttpPost]
        public ActionResult DeleteImage(int imageID)
        {
            try
            {
                // Kiểm tra xem có hình ảnh với ID cần xóa không
                var imageToDelete = db.Images.SingleOrDefault(i => i.ImageID == imageID);

                if (imageToDelete == null)
                {
                    return Json(new { code = 404, msg = "Không tìm thấy hình ảnh để xóa." });
                }

                // Lấy đường dẫn ảnh để xóa file từ thư mục
                var imagePath = Path.Combine(Server.MapPath("/Admin/Images/Room"), imageToDelete.ImagePath);

                // Xóa ảnh từ CSDL
                db.Images.DeleteOnSubmit(imageToDelete);
                db.SubmitChanges();

                // Xóa file ảnh từ thư mục
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                return Json(new { code = 200, msg = "Xóa hình ảnh thành công." });
            }
            catch (Exception e)
            {
                // Xử lý lỗi và trả về thông báo lỗi
                return Json(new { code = 500, msg = "Lỗi khi xóa hình ảnh: " + e.Message });
            }
        }

        [HttpPost]
        public ActionResult UpdateOderID(int imageID, int oderID)
        {
            try
            {
                // Kiểm tra xem có hình ảnh với ID cần cập nhật không
                var imageToUpdate = db.Images.FirstOrDefault(i => i.ImageID == imageID);

                if (imageToUpdate == null)
                {
                    return Json(new { code = 404, msg = "Không tìm thấy hình ảnh để cập nhật." });
                }

                // Cập nhật giá trị Oder ID
                imageToUpdate.OderID = oderID;
                db.SubmitChanges();

                return Json(new { code = 200, msg = "Cập nhật Oder ID thành công." });
            }
            catch (Exception e)
            {
                // Xử lý lỗi và trả về thông báo lỗi
                return Json(new { code = 500, msg = "Lỗi khi cập nhật Oder ID: " + e.Message });
            }
        }

    }
}