using System;
using System.Net.Http;
using System.Collections.Generic; 
public struct notification_data {
    public string to;
    public notification notification;
    public data data;
}
public struct notification {
        public string title;
        public string body;
}
public class data {
        public int action;
        public int vehicle_id;
        public int user_id;

        public string OTK;
        public data (int action, int user_id, int vehicle_id, string OTK) {
                this.action = action;
                this.user_id = user_id;
                this.vehicle_id = vehicle_id;
                this.OTK = OTK;
        }
}