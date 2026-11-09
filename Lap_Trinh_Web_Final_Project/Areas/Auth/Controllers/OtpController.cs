using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Lap_Trinh_Web_Final_Project.Models;

namespace Lap_Trinh_Web_Final_Project.Areas.Auth.Controllers
{
    public class OtpController : Controller
    {
        AccountService accountService = new AccountService();
        OtpService otpService = new OtpService();

        // ---------- Các hàm nhỏ dùng nội bộ ----------

        // Chỉ chấp nhận "reset", còn lại coi là "register"
        private string ChuanHoaFlow(string flow)
        {
            return flow == "reset" ? "reset" : "register";
        }

        // Giá trị purpose lưu trong bảng Otps
        private string MucDich(string flow)
        {
            return flow == "reset" ? "reset_password" : "register";
        }

        // Lấy tài khoản đang xác thực từ Session. Không hợp lệ thì trả về null.
        private Account LayTaiKhoan(string flow)
        {
            object idDaLuu;
            string vaiTroCanCo;

            if (flow == "reset")
            {
                idDaLuu = Session["ForgotAccountId"];
                string portal = Session["ForgotPortal"] as string;
                vaiTroCanCo = (portal == "admin") ? "admin" : "user";
            }
            else
            {
                idDaLuu = Session["RegisterAccountId"];
                vaiTroCanCo = "user";
            }

            if (idDaLuu == null) return null;

            Account acc = accountService.TimTheoId(System.Convert.ToInt64(idDaLuu));
            if (acc == null || acc.Role != vaiTroCanCo) return null;

            return acc;
        }

        // Không có phiên xác thực thì đưa về bước đầu của luồng
        private ActionResult VeBuocDau(string flow)
        {
            if (flow == "reset")
            {
                string portal = Session["ForgotPortal"] as string ?? "store";
                // Cùng area Auth nên không cần ghi area
                return RedirectToAction("Email", "ForgotPassword", new { portal = portal });
            }

            // Register nằm ở area Store
            return RedirectToAction("Index", "Register", new { area = "Store" });
        }

        // Gán dữ liệu cho view Otp/Index
        private void ChuanBiView(string flow, string email)
        {
            ViewBag.Flow = flow;
            ViewBag.Email = email;
        }

        // ---------- Các action ----------

        // GET /Auth/Otp?flow=register  hoặc  /Auth/Otp?flow=reset
        [HttpGet]
        public ActionResult Index(string flow)
        {
            flow = ChuanHoaFlow(flow);

            Account acc = LayTaiKhoan(flow);
            if (acc == null) return VeBuocDau(flow);

            ChuanBiView(flow, acc.Email);
            return View();
        }

        // POST /Auth/Otp/Verify: kiểm tra mã người dùng nhập
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Verify(string flow, string otpCode)
        {
            flow = ChuanHoaFlow(flow);

            Account acc = LayTaiKhoan(flow);
            if (acc == null) return VeBuocDau(flow);

            List<string> loi = new List<string>();
            string ma = (otpCode ?? "").Trim();

            if (ma == "")
            {
                loi.Add("Vui lòng nhập mã OTP.");
            }
            else if (!Regex.IsMatch(ma, @"^\d{6}$"))
            {
                loi.Add("Mã OTP phải gồm đúng 6 chữ số.");
            }
            else
            {
                string ketQua = otpService.XacMinh(acc.AccountId, MucDich(flow), ma);
                if (ketQua != null) loi.Add(ketQua);
            }

            if (loi.Count > 0)
            {
                ChuanBiView(flow, acc.Email);
                ViewBag.Loi = loi;
                return View("Index");
            }

            // OTP đúng: đánh dấu đã xác minh rồi sang bước tiếp theo
            if (flow == "reset")
            {
                Session["ForgotVerified"] = true;
                return RedirectToAction("Password", "ForgotPassword");
            }

            Session["RegisterVerified"] = true;
            return RedirectToAction("Password", "Register", new { area = "Store" });
        }

        // POST /Auth/Otp/Resend: gửi lại mã mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Resend(string flow)
        {
            flow = ChuanHoaFlow(flow);

            Account acc = LayTaiKhoan(flow);
            if (acc == null) return VeBuocDau(flow);

            string loiGui = otpService.TaoVaGui(acc, MucDich(flow));
            if (loiGui != null)
            {
                ChuanBiView(flow, acc.Email);
                ViewBag.Loi = new List<string> { loiGui };
                return View("Index");
            }

            TempData["ThanhCong"] = "Đã gửi mã OTP mới. Vui lòng kiểm tra email.";
            return RedirectToAction("Index", new { flow = flow });
        }
    }
}