﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorApp.Shared.Models
{
    public class NotificationSubscription
    {
        public int NotificationSubscriptionId { get; set; }

        public string UserId { get; set; }

        public string Url { get; set; }

        public string P256dh { get; set; }

        public string Auth { get; set; }
    }
}
