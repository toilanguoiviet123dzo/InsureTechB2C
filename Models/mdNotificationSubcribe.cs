using MongoDB.Bson;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Models
{
    [Collection("NotificationSubcribe")]
    public class mdNotificationSubcribe : Entity
    {
        public int NotificationSubscriptionId { get; set; }
        public string UserId { get; set; } = "";
        public string Url { get; set; } = "";
        public string P256dh { get; set; } = "";
        public string Auth { get; set; } = "";
        public DateTime ModifiedOn { get; set; }
        public int UpdMode { get; set; }
    }
}
