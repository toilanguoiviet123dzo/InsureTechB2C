using MongoDB.Bson;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Database.Models

{
    [Collection("FirebasePushMessageQueue")]
    public class mdFirebasePushMessageQueue : Entity
    {
        public DateTime QueueTime { get; set; } 
        public DateTime SentTime { get; set; } 
        public DateTime ExpiredTime { get; set; } 
        public string TopicID { get; set; } = "";
        public string UserName { get; set; } = "";
        public string AppToken { get; set; } = "";
        public string Title { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string Body { get; set; } = "";
        public string MessageData { get; set; } = "";
        public string Options { get; set; } = "";
    }
}
