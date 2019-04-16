using System;
public class login_response
{
    public int status;
    public int id;
    public string login_hash;
    public login_response() {}
    public login_response(int status, int id, string login_hash) {
        this.status = status;
        this.id = id;
        this.login_hash = login_hash;
    }
}