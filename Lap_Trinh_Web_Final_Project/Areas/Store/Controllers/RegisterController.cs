using System.Collections.Generic;
using System.Web.Mvc;
using Lap_Trinh_Web_Final_Project.Models;

namespace Lap_Trinh_Web_Final_Project.Areas.Store.Controllers
{
    public class RegisterController : Controller
    {
        AccountService accountService = new AccountService();
        OtpService otpService = new OtpService();

        // ---------- Các hàm nhỏ dùng nội bộ ----------

        private void XoaPhienDangKy()
        {
            Session.Remove("RegisterAccountId");
            Session.Remove("RegisterVerified");
        }

        // Lấy tài khoản tạm đã xác minh OTP. Chưa xác minh thì trả về null.
        private Account LayTaiKhoanDaXacMinhOtp()
        {
            object id = Session["RegisterAccountId"];
            bool daXacMinh = Session["RegisterVerified"] != null && (bool)Session["RegisterVerified"];
            if (id == null || !daXacMinh) return null;

            Account acc = accountService.TimTheoId(System.Convert.ToInt64(id));
            if (acc == null || acc.Role != "user" || acc.Status != "active" || acc.IsVerified) return null;

            return acc;
        }

        // ---------- Bước 1: nhập email ----------

        // GET /Store/Register
        [HttpGet]
        public ActionResult Index()
        {
            XoaPhienDangKy();
            return View();
        }

        // POST /Store/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string email)
        {
            email = (email ?? "").Trim().ToLowerInvariant();

            List<string> loi = new List<string>();
            if (email == "")
            {
                loi.Add("Vui lòng nhập email.");
            }
            else if (email.Length > 100)
            {
                loi.Add("Email không được vượt quá 100 ký tự.");
            }
            else if (!KiemTra.LaEmailHopLe(email))
            {
                loi.Add("Email không đúng định dạng.");
            }

            // Lấy hoặc tạo tài khoản tạm. Trả về null nghĩa là email đã đăng ký xong.
            Account acc = null;
            if (loi.Count == 0)
            {
                acc = accountService.LayHoacTaoTaiKhoanTam(email);
                if (acc == null)
                {
                    loi.Add("Email này đã được đăng ký. Vui lòng đăng nhập hoặc dùng email khác.");
                }
                else if (acc.Status != "active")
                {
                    acc = null;
                    loi.Add("Tài khoản với email này hiện không thể đăng ký.");
                }
            }

            // Gửi OTP
            if (loi.Count == 0)
            {
                string loiGui = otpService.TaoVaGui(acc, "register");
                if (loiGui != null) loi.Add(loiGui);
            }

            if (loi.Count > 0)
            {
                ViewBag.Email = email;
                ViewBag.Loi = loi;
                return View();
            }

            // Lưu phiên để các bước sau dùng đúng tài khoản này
            XoaPhienDangKy();
            Session["RegisterAccountId"] = acc.AccountId;
            Session["RegisterVerified"] = false;

            TempData["ThanhCong"] = "Mã OTP đã được gửi đến email của bạn.";
            return RedirectToAction("Index", "Otp", new { area = "Auth", flow = "register" });
        }

        // ---------- Bước 3: họ tên + mật khẩu (sau khi OTP đúng) ----------

        // GET /Store/Register/Password
        [HttpGet]
        public ActionResult Password()
        {
            if (LayTaiKhoanDaXacMinhOtp() == null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        // POST /Store/Register/Password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Password(string fullName, string password, string confirmPassword)
        {
            Account acc = LayTaiKhoanDaXacMinhOtp();
            if (acc == null)
            {
                return RedirectToAction("Index");
            }

            fullName = (fullName ?? "").Trim();

            List<string> loi = new List<string>();
            if (fullName == "")
            {
                loi.Add("Vui lòng nhập họ tên.");
            }
            else if (fullName.Length > 255)
            {
                loi.Add("Họ tên không được vượt quá 255 ký tự.");
            }

            if (string.IsNullOrEmpty(password))
            {
                loi.Add("Vui lòng nhập mật khẩu.");
            }
            else if (password.Length < 8)
            {
                loi.Add("Mật khẩu phải có ít nhất 8 ký tự.");
            }
            else if (password != confirmPassword)
            {
                loi.Add("Xác nhận mật khẩu không khớp.");
            }

            if (loi.Count > 0)
            {
                ViewBag.FullName = fullName;
                ViewBag.Loi = loi;
                return View();
            }

            string hash = BCrypt.Net.BCrypt.HashPassword(password);
            bool ok = accountService.HoanTatDangKy(acc.AccountId, fullName, hash);
            if (!ok)
            {
                XoaPhienDangKy();
                return RedirectToAction("Index");
            }

            XoaPhienDangKy();
            TempData["DangKyXong"] = true;
            return RedirectToAction("Success");
        }

        // ---------- Hoàn tất ----------

        // GET /Store/Register/Success
        [HttpGet]
        public ActionResult Success()
        {
            // Vào thẳng trang này (hoặc F5) thì quay về bước 1
            if (TempData["DangKyXong"] == null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}