using LuxuryHotel.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using PagedList.Mvc;
using System.Drawing;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Data.Linq;
using System.Web.UI;

namespace LuxuryHotel.Controllers
{
    public class LuxuryHotelController : Controller
    {
         dbDataContext db = new dbDataContext();
        // GET: LuxuryHotel
        public ActionResult Index(int? page)
        {

            var listPhong = LayPhong();
            int pageNum = (page ?? 1);
            int pageSize = 6;
            ViewBag.RoomType = db.ROOMTYPEs.ToList();
            return View(listPhong.ToPagedList(pageNum, pageSize));     
        }

        public ActionResult SearchPhong(int? size, int? page, string sortProperty, string sortOrder, string strSearch, int typeRoomID = 0)
        {
            List<SelectListItem> items = new List<SelectListItem>
    {
        new SelectListItem { Text = "3", Value = "3" },
        new SelectListItem { Text = "5", Value = "5" },
        new SelectListItem { Text = "10", Value = "10" },
        new SelectListItem { Text = "20", Value = "20" },
        new SelectListItem { Text = "25", Value = "25" },
        new SelectListItem { Text = "50", Value = "50" }
    };

            ViewBag.size = items;
            ViewBag.currentSize = size;
            ViewBag.Search = strSearch;

            int iSize = (size ?? 3);
            int iPageNumber = (page ?? 1);

            var kq = from s in db.ROOMs select s;

            if (!string.IsNullOrEmpty(strSearch))
            {
                kq = kq.Where(s => s.RoomName.Contains(strSearch));
            }

            if (!string.IsNullOrEmpty(sortProperty))
            {
                if (sortOrder == "desc")
                    kq = kq.OrderBy(sortProperty + " desc");
                else
                    kq = kq.OrderBy(sortProperty);
            }

            if (typeRoomID != 0)
            {
                kq = kq.Where(s => s.ROOMTYPE.RoomTypeID == typeRoomID);

            }

            ViewBag.typeRoomID = new SelectList(db.ROOMTYPEs, "RoomTypeID", "TypeName");

            ViewBag.SortOrder = sortOrder;
            ViewBag.SortProperty = sortProperty;

            return View(kq.ToPagedList(iPageNumber, iSize));
        }
        public ActionResult TimKiem(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                // Nếu từ khóa tìm kiếm là null hoặc chuỗi rỗng, bạn có thể xử lý nó ở đây (ví dụ: hiển thị thông báo lỗi).
                // Sau đó trả về trang tìm kiếm hoặc trang chính tùy theo yêu cầu của bạn.
                return View("Index");
            }

            var query = from kv in db.ROOMs
                        where kv.RoomName.Contains(s) ||
                              kv.RoomStatus.Contains(s) ||
                             kv.RoomTypeID.ToString().Contains(s) ||
                              kv.ROOMTYPE.TypeName.Contains(s) ||
                              kv.Area.Contains(s)
                        select kv;

            var kvs = query.ToList(); // Chuyển kết quả thành danh sách

            // Trả về view TimKiem và truyền danh sách kết quả tìm kiếm
            return View("TimKiem", kvs);
        }

        public ActionResult ViewSearch(string searchString, int typeRoomID = 0)
        {
            var kq = db.ROOMs.Include(b => b.ROOMTYPE);

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                kq = kq.Where(b => b.RoomStatus.ToLower().Contains(searchString) ||
                                  b.RoomName.ToLower().Contains(searchString) ||
                                  b.ROOMTYPE.TypeName.ToLower().Contains(searchString));
            }

            if (typeRoomID != 0)
            {
                kq = kq.Where(b => b.ROOMTYPE.RoomTypeID == typeRoomID);
            }

            ViewBag.RoomTypeID = new SelectList(db.ROOMTYPEs, "RoomTypeID", "TypeName");

            return View(kq.ToList());
        }

        private  Utility LayTienIch(int UtilitiesID)
        {
            return db.Utilities.Where(a => a.UtilitiesID== UtilitiesID).SingleOrDefault(); ;
        }
      
        private List<LuxuryHotel.Models.ROOM> LayPhong()
        {
            return db.ROOMs.OrderByDescending(a => a.RoomID).ToList();
        }
        public ActionResult ChiTietPhong(int id)
        {
            var ListImage = from b in db.Images where b.RoomID == id select b;
            ViewBag.ImagesRoom = ListImage.ToList(); // Convert to List
            var area = (from c in db.ROOMs where c.RoomID == id select c).SingleOrDefault(); ;
            ViewBag.Area = area.Area; // Convert to List
            var utilities = from d in db.RoomUtilities where d.RoomID == id select d;
            List<LuxuryHotel.Models.Utility> lst = new List<LuxuryHotel.Models.Utility>();
            foreach (var x in utilities)
            {
                var uti = LayTienIch(x.UtilitiesID);
                lst.Add(uti);
            }
            ViewBag.Utulities = lst.ToList(); 
            var room = (from s in db.ROOMs where s.RoomID == id select s).SingleOrDefault();
            var roomtype = (from e in db.ROOMTYPEs where e.RoomTypeID == room.RoomTypeID select e).SingleOrDefault();
            ViewBag.TypeName = roomtype.TypeName;
            ViewBag.PricePerDay = roomtype.PriceByDay;
            ViewBag.PriceFirstHour = roomtype.PriceFirstHour;
            ViewBag.PricePerHour = roomtype.PricePerHour;
            ViewBag.OverNightPrice = roomtype.OverNightPrice;
      
            return View(room);
        }
        
        public ActionResult SliderPartial()
        {
            return PartialView();
        }
        public ActionResult Sliderroom(int id)
        {
            var list = db.Images
                   .Where(r => r.RoomID == id)
                   .ToList();
            return PartialView(list);
        }
        public ActionResult LoginLogout()
        {

            return PartialView();
        }
        public ActionResult Search(string strSearch)
        {
            Console.WriteLine("Đang vào action Search"); // Hiển thị thông điệp trong Output

            ViewBag.Search = strSearch;

            if (string.IsNullOrEmpty(strSearch))
            {
                Console.WriteLine("Chuỗi tìm kiếm rỗng");
                return View();
            }

            var kq = db.ROOMs.Where(s => s.Area.Contains(strSearch));
            Console.WriteLine(kq);
            ViewBag.Kq = kq.Count();
            return View(kq);
        }

    }
}