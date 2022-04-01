using MongoDB.Bson;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp.Server.Models
{
    [Collection("ResourceFile")]
    public class mdResourceFile : Entity
    {
        public string OwnerID { get; set; } = "";
        public string CategoryID { get; set; } = "";
        public string ResourceID { get; set; } = "";
        public string FileType { get; set; } = "";
        public string Title { get; set; } = "";
        public string FileName { get; set; } = "";
        public string ServerFileName { get; set; } = "";
        public string ServerThumbnailFileName { get; set; } = "";
        public byte[] FileContent { get; set; } = new byte[] { };
        public byte[] Thumbnail { get; set; } = new byte[] { };
        public bool IsImage { get; set; }
        public bool IsMakeFullImage { get; set; }
        public bool IsMakeThumbnail { get; set; }
        public bool HasThumbnail { get; set; }
        public bool HasFullImage { get; set; }
        public int ThumbnailWidth { get; set; }
        public int ThumbnailHeight { get; set; }
        public int ArchiveMode { get; set; }
        public int SecureLevel { get; set; }
        public string AccountID { get; set; } = "";
        public DateTime IssueDate { get; set; }
        public int UpdMode { get; set; }
    }
}
