using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Client.BindingModels
{
    public class UpLoadFileModel
    {
        public string ID { get; set; } = "";
        public string OwnerID { get; set; } = "";
        public string CategoryID { get; set; } = "";
        public string ResourceID { get; set; } = "";
        public string FileType { get; set; } = "";
        public string Title { get; set; } = "";
        public string FileName { get; set; } = "";
        public byte[] FileContent { get; set; } = new byte[] { };
        public byte[] Thumbnail { get; set; } = new byte[] { };
        public bool IsMakeFullImage { get; set; }
        public bool IsMakeThumbnail { get; set; }
        public int ThumbnailWidth { get; set; }
        public int ThumbnailHeight { get; set; }
        public bool HasThumbnail { get; set; }
        public bool HasFullImage { get; set; }
        public int SecureLevel { get; set; }
        public string SecureLevelName { get; set; }
        public string AccountID { get; set; } = "";
        public DateTime IssueDate { get; set; }
        public int UpdMode { get; set; }
        public string RecNo { get; set; } = "";
    }
}
