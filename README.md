# Công dụng các thư mục trong project

Tài liệu này mô tả cấu trúc project ASP.NET MVC 5 bán linh kiện điện tử. Project được chia thành hai khu vực giao diện chính: **Store** dành cho khách mua hàng và **Admin** dành cho quản trị viên; khu vực **Auth** xử lý xác thực dùng chung.

## Cấu trúc tổng quan

```text
Lap_Trinh_Web_Final_Project/
├── Areas/
│   ├── Admin/
│   ├── Auth/
│   └── Store/
├── App_Data/
├── App_Start/
├── Content/
├── Models/
├── Scripts/
├── Services/
├── Uploads/
├── Views/
├── Properties/
├── Web.config
├── Global.asax
└── Lap_Trinh_Web_Final_Project.csproj
```

## Các thư mục trong project

| Thư mục | Công dụng | Ví dụ nội dung |
|---|---|---|
| `Areas/` | Chia ứng dụng thành các khu vực lớn, mỗi khu vực có controller, view và cấu hình route riêng. | `Admin`, `Auth`, `Store` |
| `Areas/Store/` | Chức năng dành cho khách: trang chủ, xem sản phẩm, đăng nhập và đăng ký. | `Controllers`, `ViewModels`, `Views` |
| `Areas/Admin/` | Khu vực quản trị: dashboard, quản lý sản phẩm và danh mục. | `Controllers`, `ViewModels`, `Views` |
| `Areas/Auth/` | Các luồng xác thực riêng như OTP và quên mật khẩu. | `Controllers`, `Views` |
| `Controllers/` | Controller cấp ứng dụng nếu có chức năng không thuộc một Area cụ thể. Project hiện tổ chức phần lớn controller bên trong `Areas`. | Có thể để controller dùng chung nếu sau này cần. |
| `ViewModels/` | Các lớp dữ liệu được thiết kế riêng để truyền dữ liệu giữa controller và view; không nhất thiết trùng cấu trúc bảng database. | `ProductCreateViewModel`, `StoreHomeViewModel` |
| `Views/` | Các Razor view không thuộc Area và các view dùng chung cấp ứng dụng. | `Shared` |
| `Views/Shared/` | Layout, partial view hoặc thành phần giao diện dùng chung toàn ứng dụng. | Trang lỗi, partial dùng chung |
| `Areas/*/Controllers/` | Nhận yêu cầu HTTP, kiểm tra dữ liệu đầu vào, gọi service và trả về view/redirect. | `HomeController`, `ProductsController` |
| `Areas/*/Views/` | Giao diện Razor của từng controller trong Area đó; thường đặt trong thư mục trùng tên controller. | `Areas/Store/Views/Home/Index.cshtml` |
| `Areas/*/ViewModels/` | ViewModel chỉ phục vụ khu vực tương ứng. | `Areas/Admin/ViewModels/ProductListItem.cs` |
| `Models/` | Entity Framework EDMX, entity sinh từ database và lớp mô hình dùng chung. Hạn chế sửa trực tiếp file được EDMX sinh lại. | `Lap_Trinh_Web_Final_Project.edmx`, `Product.cs` |
| `Services/` | Chứa nghiệp vụ và thao tác dữ liệu dùng lại giữa các controller; giúp controller không phải chứa toàn bộ truy vấn SQL/EF. | `ProductService`, `CategoryService`, `StoreProductService` |
| `Content/` | CSS, hình giao diện và tài nguyên tĩnh. CSS thường được chia theo khu vực. | `Content/store`, `Content/admin`, `Content/auth` |
| `Scripts/` | JavaScript phía trình duyệt, cũng được chia theo khu vực. | `Scripts/store`, `Scripts/admin`, `Scripts/auth` |
| `Uploads/` | File người dùng tải lên và được website lưu lại. Ảnh sản phẩm nằm trong `Uploads/products/`. | `Uploads/products/ten-anh.jpg` |
| `App_Start/` | Cấu hình khởi động MVC như route, filter, bundle và đăng ký Area. | `RouteConfig.cs`, `BundleConfig.cs` |
| `App_Data/` | Dữ liệu cục bộ của ứng dụng khi cần; không phải nơi lưu ảnh sản phẩm có URL công khai. | File dữ liệu nội bộ |
| `Properties/` | Cấu hình project và thông tin assembly của Visual Studio. | `AssemblyInfo.cs`, cấu hình debug |
| `bin/` | File chương trình đã build và thư viện chạy. Đây là đầu ra build, không sửa mã nguồn tại đây. | DLL, Roslyn compiler |
| `obj/` | File trung gian do quá trình build tạo ra. Có thể được Visual Studio tạo lại. | File tạm theo cấu hình Debug/Release |

## Một số file ở thư mục gốc

| File | Công dụng |
|---|---|
| `Global.asax` và `Global.asax.cs` | Điểm khởi chạy ứng dụng ASP.NET; đăng ký Area, route, bundle và cấu hình ban đầu. |
| `Web.config` | Cấu hình ứng dụng: connection string, app settings, xác thực, runtime và các thiết lập ASP.NET. Không đưa mật khẩu thật lên Git. |
| `Web.Debug.config`, `Web.Release.config` | Các thay đổi cấu hình áp dụng khi chạy Debug hoặc publish Release. |
| `Lap_Trinh_Web_Final_Project.csproj` | File project của Visual Studio; liệt kê mã nguồn, view, tài nguyên và các tham chiếu. Với project MVC kiểu cũ, file mới có thể cần được khai báo trong `.csproj`. |
| `Lap_Trinh_Web_Final_Project.csproj.user` | Thiết lập cá nhân trên máy như cấu hình khởi chạy Visual Studio; thường không dùng làm cấu hình chung cho cả nhóm. |
| `packages.config` | Danh sách package NuGet của project kiểu .NET Framework cũ. |
| `favicon.ico` | Biểu tượng của website hiển thị trên tab trình duyệt. |

## Quy tắc chọn nơi đặt file mới

- Controller của Store đặt trong `Areas/Store/Controllers/`; controller Admin đặt trong `Areas/Admin/Controllers/`.
- View đặt trong `Areas/<Area>/Views/<TênController>/` và tên file thường trùng tên action. Ví dụ action `Details` của `HomeController` dùng `Areas/Store/Views/Home/Details.cshtml`.
- ViewModel dùng riêng cho một Area thì đặt trong `Areas/<Area>/ViewModels/`; ViewModel thực sự dùng chung có thể đặt ở thư mục `ViewModels/` cấp ứng dụng nếu project có thư mục đó.
- Nghiệp vụ/truy vấn tái sử dụng đặt trong `Services/`; không đặt truy vấn database dài trong Razor view.
- CSS và JavaScript đặt trong `Content/<area>/` và `Scripts/<area>/` tương ứng.
- Ảnh tải lên nên lưu trong `Uploads/` và database chỉ lưu đường dẫn tương đối, chẳng hạn `Uploads/products/phone-01.jpg`.
- Nếu thêm file vào project MVC 5 kiểu cũ, kiểm tra `.csproj`: thêm file C# dưới dạng `Compile` và file Razor/tĩnh dưới dạng `Content` khi cần.
- Không sửa thủ công file trong `bin/` hoặc `obj/`; không chỉnh file entity sinh từ EDMX trừ khi cập nhật đúng nguồn EDMX/database.

## Luồng xử lý điển hình

```text
Trình duyệt
    ↓
Area Controller
    ↓
Service (nghiệp vụ và truy vấn)
    ↓
Entity Framework / Models
    ↓
SQL Server
    ↓
ViewModel
    ↓
Razor View trong Area tương ứng
```

`Content/` và `Scripts/` cung cấp giao diện phía trình duyệt; `Uploads/` chứa ảnh/file mà ứng dụng phục vụ; `Web.config` và `App_Start/` cấu hình cách ứng dụng kết nối và định tuyến.
