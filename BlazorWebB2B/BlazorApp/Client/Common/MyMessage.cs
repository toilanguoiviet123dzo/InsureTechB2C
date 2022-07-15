using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp.Client.Common
{
    public static class MyMessage
    {
        //Error message
        public const string Error_ServerError = "Máy chủ tạm thời ngưng phục vụ, xin hãy quay lại sau.";
        public const string Error_LoadDataFailed = "Load dữ liệu thất bại";
        public const string Error_SaveFailed = "Lưu dữ liệu thất bại";
        public const string Error_DeleteFailed = "Xóa dữ liệu thất bại";
        public const string Error_NoNetwork = "Không kết nối được với máy chủ";
        public const string Error_UploadFile = "Upload file thất bại";
        public const string Error_DownloadFile = "Download file thất bại!";
        public const string Error_LoadFile = "Load file bị lỗi!";
        public const string Error_DeleteFile = "Xóa file thất bại";
        public const string Error_ImportFile = "Import file thất bại";

        //Warnning
        public const string Warning_NoData = "Không có dữ liệu";
        public const string Warning_DataReadOnly = "Không cho phép chỉnh sửa";
        public const string Warning_DeleteReadOnlyData = "Không cho phép xóa dữ liệu";

        //Confirm message
        public const string Confirm_DeleteRow = "Xóa dòng dữ liệu?";

        //Infor
        public const string Info_SaveSucess = "Lưu dữ liệu thành công";
        public const string Info_ImportSucess = "Import dữ liệu thành công";
        public const string Info_DeleteSucess = "Xóa dữ liệu thành công";
        public const string Info_NothingChanged = "Không có dữ liệu thay đổi";

    }// end class
}
