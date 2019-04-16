using System;
public class user
{
    public int id;
    public string first_name;
    public string last_name;
    public string email;
    public string licence_number;
    public user() {}
    public user(int id, string first_name, string last_name, string email, string licence_number) {
        this.id = id;
        this.first_name = first_name;
        this.last_name = last_name;
        this.email = email;
        this.licence_number = licence_number;
    }
}