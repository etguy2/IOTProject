using System;
public class log
{
    public int id;
    public string message;
    public DateTime time;
    public log() {}
    public log(int id, string message, DateTime time) {
        this.setData(id, message, time);
    }
    public void setData(int id, string message, DateTime time) {
        this.id = id;
        this.message = message;
        this.time = time;
    }
}