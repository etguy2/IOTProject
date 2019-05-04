using System;    
using System.Text;  
using System.Linq;
using System.Net.Http.Formatting;
using System.Data.SqlClient;

public class car_partial {
    public int id;
    public string lat;
    public string lng;
    public string manufacturer;

    public int mode;
    public string model;

    public int year;
    public car_partial() {}
    public car_partial(SqlDataReader reader) { 
        this.id = (int)reader["id"];
        this.year = (int)reader["year"];
        this.lat = (string)reader["lat"];
        this.lng = (string)reader["lng"];
        this.manufacturer = (string)reader["manufacturer"];
        this.mode = (int)reader["mode"];
        this.model = (string)reader["model"];
    }
}
public class car_full : car_partial {
    public string img;
    public string MACID;

    public user user;

    public car_full() {}
    public car_full(SqlDataReader reader) : base(reader) {
        this.img = utilitles.safeCast(reader, "carimage");
        this.MACID = (string)reader["MACID"];
        this.user = new user(); 
        this.user.id = (int)reader["ownerid"];
        this.user.email = utilitles.safeCast(reader, "owneremail");
        this.user.img = utilitles.safeCast(reader, "ownerimg");
        this.user.first_name = utilitles.safeCast(reader, "owner_first_name");
        this.user.last_name = utilitles.safeCast(reader, "owner_last_name");
    }
}