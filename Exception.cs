using System;    
using System.Text;  
using System.Linq;
using System.Net.Http.Formatting;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
namespace CarSharing.Exceptions
{
    [ComVisible(true)]
    public class CarSharingException : System.IO.IOException
    {
        public int status_code;
        public string info;

        public CarSharingException() { this.status_code = 0; }
        public CarSharingException(int status_code, string info) {
            this.status_code = status_code;
            this.info = info;
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
        public VehicleNotFound(string message) : base(-3, "Vehicle no. '"+message+"' not found in the DB.") {}
    }
    class NoPermit : CarSharingException {
        int user_id;
        int vehicle_id;
        public NoPermit() : base(-4, "No Permit.") {}
        public NoPermit(int user_id, int vehicle_id) : base(-3, "User #"+user_id.ToString()+" Has no permit for car #"+vehicle_id.ToString()+".") {}
    }
    class PermitRequestExists : CarSharingException {
        int user_id;
        int vehicle_id;
        public PermitRequestExists() : base(-5, "Permit request exists.") {}
        public PermitRequestExists(int user_id, int vehicle_id) : base(-3, "User #"+user_id.ToString()+" Already have a permit request for car #"+vehicle_id.ToString()+".") {}
    }
}