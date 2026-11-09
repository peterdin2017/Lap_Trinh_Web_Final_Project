using Lap_Trinh_Web_Final_Project.Models;
using System.Collections.Generic;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Lap_Trinh_Web_Final_Project.Areas.Auth.Controllers
{
    public class ForgotPasswordController : Controller
    {
        AccountService accountService = new AccountService();
        OtpService otpService = new OtpService();

        // ---------- Các hàm nhỏ dùng nội bộ ----------

        // portal chỉ có 2 giá trị: "store" hoặc "admin"
        private string ChuanHoaPortal(string portal)
        {
            return portal == "admin" ? "admin" : "store";
        }

        // Portal nào thì role đó trong bảng Accounts
        private string VaiTroCuaPortal(string portal)
        {
            return portal == "admin" ? "admin" : "user";
        }

        // Portal đang lưu trong Session (mặc định store)
        private string LayPortal()
        {
            return ChuanHoaPortal(Session["ForgotPortal"] as string);
        }

        private void XoaPhienQuenMatKhau()
        {
            Session.Remove("ForgotAccountId");
            Session.Remove("ForgotVerified");
            Session.Remove("ForgotPortal");
        }

        // Lấy tài khoản đã xác minh OTP. Chưa xác minh thì trả về null.
        private Account LayTaiKhoanDaXacMinhOtp()
        {
            object id = Session["ForgotAccountId"];
            bool daXacMinh = Session["ForgotVerified"] != null && (bool)Session["ForgotVerified"];
            if (id == null || !daXacMinh) return null;

            Account acc = accountService.TimTheoId(System.Convert.ToInt64(id));
            if (acc == null) return null;
            if (acc.Role != VaiTroCuaPortal(LayPortal())) return null;
            if (acc.Status != "active" || !acc.IsVerified) return null;

            return acc;
        }

        // ---------- Bước 1: nhập email ----------

        // GET /Auth/ForgotPassword/Email?portal=store
        [HttpGet]
        public ActionResult Email(string portal)
        {
            ViewBag.Portal = ChuanHoaPortal(portal);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Email(string email, string portal)
        {
            portal = ChuanHoaPortal(portal);
            email = (email ?? "").Trim().ToLowerInvariant();

            // Kiểm tra dữ liệu nhập
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

            // Tìm đúng tài khoản theo email + role, phải còn hoạt động và đã xác minh
            Account acc = null;
            if (loi.Count == 0)
            {
                acc = accountService.TimTheoEmail(email, VaiTroCuaPortal(portal));
                if (acc == null || acc.Status != "active" || !acc.IsVerified)
                {
                    loi.Add("Không tìm thấy tài khoản đã xác minh với email này.");
                }
            }

            // Gửi OTP
            if (loi.Count == 0)
            {
                string loiGui = otpService.TaoVaGui(acc, "reset_password");
                if (loiGui != null) loi.Add(loiGui);
            }

            if (loi.Count > 0)
            {
                ViewBag.Portal = portal;
                ViewBag.Email = email;
                ViewBag.Loi = loi;
                return View();
            }

            // Lưu vào Session để các bước sau dùng đúng tài khoản này
            XoaPhienQuenMatKhau();
            Session["ForgotAccountId"] = acc.AccountId;
            Session["ForgotVerified"] = false;
            Session["ForgotPortal"] = portal;

            TempData["ThanhCong"] = "Mã OTP đã được gửi đến email của bạn.";
            return RedirectToAction("Index", "Otp", new { flow = "reset" });
        }

        // ---------- Bước 3: mật khẩu mới (sau khi OTP đúng) ----------

        [HttpGet]
        public ActionResult Password()
        {
            if (LayTaiKhoanDaXacMinhOtp() == null)
            {
                return RedirectToAction("Email", new { portal = LayPortal() });
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Password(string password, string confirmPassword)
        {
            Account acc = LayTaiKhoanDaXacMinhOtp();
            if (acc == null)
            {
                return RedirectToAction("Email", new { portal = LayPortal() });
            }

            List<string> loi = new List<string>();
            if (string.IsNullOrEmpty(password))
            {
                loi.Add("Vui lòng nhập mật khẩu mới.");
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
                ViewBag.Loi = loi;
                return View();
            }

            // Băm mật khẩu bằng BCrypt rồi lưu
            accountService.DatMatKhau(acc.AccountId, BCrypt.Net.BCrypt.HashPassword(password));

            // Nhớ portal để trang Success biết quay về đăng nhập nào, rồi xóa phiên
            TempData["ResetPortal"] = LayPortal();
            XoaPhienQuenMatKhau();

            return RedirectToAction("Success");
        }

        // ---------- Bước 4: thành công ----------

        [HttpGet]
        public ActionResult Success()
        {
            string portal = TempData["ResetPortal"] as string;

            // Mở thẳng trang này (hoặc F5) thì quay về bước 1
            if (portal == null)
            {
                return RedirectToAction("Email", new { portal = "store" });
            }

            // Login của Store và AdminLogin của Admin nằm ở area khác nên phải ghi rõ area
            ViewBag.LoginUrl = (portal == "admin")
                ? Url.Action("Index", "AdminLogin", new
                {
                    area = "Admin"
                })
                : Url.Action("Index", "Login", new
                {
                    area = "Store"
                });

            return View();
        }
    }
}