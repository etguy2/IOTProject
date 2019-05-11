using System;
using System.Collections.Generic;
public class user
{
    public int id;
    public string first_name;
    public string last_name;
    public string img;
    public string email;
    public string licence_number;
    public List<Permit_small> permits;
    public user() {}
    public user(int id, string first_name, string last_name, string email, string licence_number, string img) {
        this.id = id;
        this.first_name = first_name;
        this.last_name = last_name;
        this.email = email;
        this.licence_number = licence_number;
        this.img = img;
    }
    public void setPermits(List<Permit_small> permits) {
        this.permits = permits;
    }
}
public class Permit_small {
    public DateTime submit_time;
    public int vehicle_id;
    public string status;
    public Permit_small (int vehicle_id, string status, DateTime submit_time) {
        this.vehicle_id = vehicle_id;
        this.status = status;
        this.submit_time = submit_time;
    }
}