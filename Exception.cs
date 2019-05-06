using System;    
using System.Text;  
using System.Linq;
using System.Net.Http.Formatting;
using System.Data.SqlClient;

[Serializable()]

class CarSharingException {
    public int status {
        get { return status; }
        protected set { status = value; }
    }
    public string message {
        get { return message; }
        protected set { message = value; }
    }
    public CarSharingException(int status, string message) {
        this.status = status;
        this.message = message;
    }
}
class InvalidInputException : CarSharingException {

    public InvalidInputException() : base(-1, "invalid Input") {}
    public InvalidInputException(string message) : base(-1, "invalid Input, no '"+message+"'") {}
}

class UserNotVerified : CarSharingException {
    public UserNotVerified() : base(-2, "User not verified") {}
    public UserNotVerified(string message) : base(-2, "user no. '"+message+"' could not be verified") {}
}

class VehicleNotFound : CarSharingException {
    public VehicleNotFound() : base(-3, "Vehicle not found.") {}
    public VehicleNotFound(string message) : base(-1, "Vehicle no. '"+message+"' not found in the DB.") {}
}