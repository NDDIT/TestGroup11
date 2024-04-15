using LuxuryHotel.Models;
using LuxuryHotel.Momo;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using System.Net.Mail;
using System.Net;
using System.Configuration;
using System.Web.UI;
using IBook.Content.module;
using PagedList;
using PagedList.Mvc;
namespace LuxuryHotel.Controllers
{
    public class BookingRoomController : Controller
    {
        // GET: BookingRoom
        dbDataContext db = new dbDataContext();
       
        public ActionResult  BookForm(int RoomID , DateTime checkindate , DateTime checkoutdate , int cost)
        {
            var ListImage = from b in db.Images where b.RoomID == RoomID select b;
            ViewBag.ImagesRoom = ListImage.ToList(); // Convert to List
            var area = (from c in db.ROOMs where c.RoomID == RoomID select c).SingleOrDefault(); ;
            ViewBag.Area = area.Area; // Convert to List
            var utilities = from d in db.RoomUtilities where d.RoomID == RoomID select d;
            List<LuxuryHotel.Models.Utility> lst = new List<LuxuryHotel.Models.Utility>();
            foreach (var x in utilities)
            {
                var uti = LayTienIch(x.UtilitiesID);
                lst.Add(uti);
            }
            ViewBag.Utulities = lst.ToList();
           
            var room = (from s in db.ROOMs where s.RoomID == RoomID select s).SingleOrDefault();
            var roomtype = (from e in db.ROOMTYPEs where e.RoomTypeID == room.RoomTypeID select e).SingleOrDefault();
            ViewBag.TypeName = roomtype.TypeName;
            ViewBag.PricePerDay = roomtype.PriceByDay;
            ViewBag.PriceFirstHour = roomtype.PriceFirstHour;
            ViewBag.PricePerHour = roomtype.PricePerHour;
            ViewBag.OverNightPrice = roomtype.OverNightPrice;
            ViewBag.CheckInDate = checkindate;
            ViewBag.CheckOutDate = checkoutdate;
            ViewBag.cost = cost;
            return View(room);
        }
        private Utility LayTienIch(int UtilitiesID)
        {
            return db.Utilities.Where(a => a.UtilitiesID == UtilitiesID).SingleOrDefault(); ;
        }

        private void GuiEmailDonHang(string emailKhachHang, BOOKING bk , int cost)
        {
            try
            {
                // Địa chỉ email của người gửi và người nhận
                string Email = ConfigurationManager.AppSettings["Email"];
                string Password = ConfigurationManager.AppSettings["PasswordEmail"];
                string toEmail = emailKhachHang;
                var room = (from s in db.ROOMs where s.RoomID == bk.RoomID select s).SingleOrDefault();
                var roomtype = (from s in db.ROOMTYPEs where s.RoomTypeID == room.RoomTypeID select s).SingleOrDefault();
                // Tạo đối tượng MailMessage
                MailMessage message = new MailMessage(Email, toEmail);

                // Tiêu đề email
                message.Subject = "Xác nhận đơn hàng";

                // Nội dung email
                message.Body = $"Xin chào ,cảm ơn bạn đã đặt phòng của chúng tôi. Dưới đây là chi tiết đơn hàng của bạn:\n\n";

                
                    message.Body += $"Phòng: {room.RoomName}\n";
                    message.Body += $"loại phòng: {roomtype.TypeName}\n";
                    message.Body += $"Khu vực: {room.Area}\n";
                    message.Body += $"Thời gian đặt phòng: {bk.BookingDate}\n";
                    message.Body += $"Thời gian nhận phòng: {bk.CheckInDate}\n\n";
                    message.Body += $"Thời gian trả phòng: {bk.CheckOutDate}\n\n";
                    message.Body += $"tổng tiền: {cost:C}\n\n";


                message.Body += "Chúng tôi rất vui mừng được phục vụ bạn và mong sớm nhìn thấy bạn lần sau.\n\nCảm ơn bạn!\n";
                // Cấu hình đối tượng SmtpClient
                SmtpClient smtp = new SmtpClient("smtp.gmail.com");
                smtp.Port = 587;
                smtp.Credentials = new NetworkCredential(Email, Password); // Thay bằng thông tin tài khoản email của bạn
                smtp.EnableSsl = true;

                // Gửi email
                smtp.Send(message);
            }
            catch (Exception ex)
            {
                // Xử lý nếu có lỗi khi gửi email (có thể ghi log)
                Console.WriteLine("Lỗi khi gửi email: " + ex.Message);
            }
        }


        public ActionResult SaveBooking(int RoomID, DateTime checkindate, DateTime checkoutdate, int cost, int CustomerID)
        {
            var cus = (from s in db.CUSTOMERs where s.CustomerID == CustomerID select s).SingleOrDefault();
            BOOKING bk = new BOOKING();

            
            bk.CustomerID = CustomerID;
            bk.RoomID = RoomID;
            bk.CheckOutDate= checkoutdate;
            bk.CheckInDate= checkindate;
            bk.BookingDate= DateTime.Now;
            bk.PaymentStatus = "UnPaid";
            db.BOOKINGs.InsertOnSubmit(bk);
           
            db.SubmitChanges();
            GuiEmailDonHang(cus.Email,bk, cost);

            // Chuyển hướng đến trang xác nhận đơn hàng
            return RedirectToAction("Index", "LuxuryHotel");
        }
        public ActionResult Payment(double cost)
        {
            
            //request params need to request to MoMo system
            string endpoint = "https://test-payment.momo.vn/gw_payment/transactionProcessor";
            string partnerCode = "MOMOCJKB20220928";
            string accessKey = "9RSPhTi8yVQssm5Z";
            string serectkey = "plIPmmBjkkj89fZtFDvrmKO35WCyAMZg";
            string orderInfo = "DH" + DateTime.Now.ToString("yyyyMMddHHmmss");
            string returnUrl = "http://tdmusachonline.somee.com/GioHang/XacNhanDonHang";
            string notifyurl = "http://tdmusachonline.somee.com/GioHang/DatHang"; //lưu ý: notifyurl không được sử dụng localhost, có thể sử dụng ngrok để public localhost trong quá trình test

            string amount = cost.ToString();
            string orderid = DateTime.Now.Ticks.ToString(); //mã đơn hàng
            string requestId = DateTime.Now.Ticks.ToString();
            string extraData = "";

            //Before sign HMAC SHA256 signature
            string rawHash = "partnerCode=" +
                partnerCode + "&accessKey=" +
                accessKey + "&requestId=" +
                requestId + "&amount=" +
                amount + "&orderId=" +
                orderid + "&orderInfo=" +
                orderInfo + "&returnUrl=" +
                returnUrl + "&notifyUrl=" +
                notifyurl + "&extraData=" +
                extraData;

            MoMoSecurity crypto = new MoMoSecurity();
            //sign signature SHA256
            string signature = crypto.signSHA256(rawHash, serectkey);

            //build body json request
            JObject message = new JObject
            {
                { "partnerCode", partnerCode },
                { "accessKey", accessKey },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderid },
                { "orderInfo", orderInfo },
                { "returnUrl", returnUrl },
                { "notifyUrl", notifyurl },
                { "extraData", extraData },
                { "requestType", "captureMoMoWallet" },
                { "signature", signature }

            };

            string responseFromMomo = PaymentRequest.sendPaymentRequest(endpoint, message.ToString());

            JObject jmessage = JObject.Parse(responseFromMomo);

            return Redirect(jmessage.GetValue("payUrl").ToString());
        }

        //Khi thanh toán xong ở cổng thanh toán Momo, Momo sẽ trả về một số thông tin, trong đó có errorCode để check thông tin thanh toán
        //errorCode = 0 : thanh toán thành công(Request.QueryString["errorCode"])
        //Tham khảo bảng mã lỗi tại: https://developers.momo.vn/#/docs/aio/?id=b%e1%ba%a3ng-m%c3%a3-l%e1%bb%97i
        public ActionResult XacNhanDonHang(Result result)
        {
            //lấy kết quả Momo trả về và hiển thị thông báo cho người dùng (có thể lấy dữ liệu ở đây cập nhật xuống db)
            string rMessage = result.message;
            string rOrderId = result.orderId;
            string rErrorCode = result.errorCode; // = 0: thanh toán thành công
            return View();
        }

        [HttpPost]
        public void SavePayment()
        {
            ////cập nhật dữ liệu vào db
            //DONDATHANG ddh = new DONDATHANG();
            //KHACHHANG kh = (KHACHHANG)Session["TaiKhoan"];
            //List<GioHang> lstGioHang = LayGioHang();
            //ddh.MaKH = kh.MaKH;
            //ddh.NgayDat = DateTime.Now;
            ////var ngayGiao = string.Format("{0:MM/dd/yyyy}", f["NgayGiao"]);
            ////ddh.NgayGiao = DateTime.Parse(ngayGiao);
            //ddh.TinhTrangGiaoHang = 1;
            //ddh.DaThanhToan = false;
            //db.DONDATHANGs.InsertOnSubmit(ddh);
            //db.SubmitChanges();
            //foreach (var item in lstGioHang)
            //{
            //    CHITIETDATHANG ctdh = new CHITIETDATHANG();
            //    ctdh.MaDonHang = ddh.MaDonHang;
            //    ctdh.MaSach = item.iMaSach;
            //    ctdh.SoLuong = item.iSoLuong;
            //    ctdh.DonGia = (decimal)item.dDonGia;
            //    db.CHITIETDATHANGs.InsertOnSubmit(ctdh);
            //}
            //db.SubmitChanges();
            //Session["GioHang"] = null;
        }

    }
}