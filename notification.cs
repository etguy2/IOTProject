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
public struct data {
        public int action;
        public string message;
}