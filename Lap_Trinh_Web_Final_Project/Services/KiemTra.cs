using System;
using System.Net.Mail;

public static class KiemTra
{
    // true nếu chuỗi là email đúng định dạng
    public static bool LaEmailHopLe(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        try
        {
            MailAddress diaChi = new MailAddress(email.Trim());
            return diaChi.Address == email.Trim();
        }
        catch (FormatException)
        {
            return false;
        }
    }
}