using System;    
using System.Text;  
using System.Linq;
using System.Net.Http.Formatting;
using System.Data.SqlClient;
public struct car {
    public int id;
    public string lat;
    public string lng;
    public string manufacturer;
    public string img;

    public int mode;
    public string model;

    public user user;

    public car(SqlDataReader reader) {
        this.id = (int)reader["id"];
        this.lat = (string)reader["lat"];
        this.lng = (string)reader["lng"];
        this.manufacturer = (string)reader["manufacturer"];
        this.mode = (int)reader["mode"];
        this.model = (string)reader["model"];
        this.img = (string)reader["carimage"];
        this.user = new user();
        this.user.id = (int)reader["ownerid"];
        this.user.email = (string)reader["owneremail"]; 
        this.user.first_name = (string)reader["user_first_name"];
        this.user.last_name = (string)reader["user_last_name"];
    }
}