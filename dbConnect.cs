using System.Data.SqlClient;
using System.Collections.Generic;
using System;

// Manages all the communications with the database.
public class dbConnect
{
    private string _conn_str;
    public dbConnect() {
        // Gets a global var containing the credentials in order to login to the database.
        _conn_str = System.Environment.GetEnvironmentVariable("sqldb_connection");
    }
    // Use this method for count queries.
    public int query_scalar(string query)
    {
        using (SqlConnection conn = new SqlConnection(_conn_str))
        {
            conn.Open();
            SqlCommand command = new SqlCommand(query, conn);
            int rows = (int) command.ExecuteScalar();
            return rows;
            conn.Close();
            
        }
    }
    public List<log> get_logs(string limit) {
        List<log> logs = new List<log>();
        string query =  "SELECT top "+limit+" * FROM Logs ORDER BY id DESC";

        using (SqlConnection conn = new SqlConnection(_conn_str)) {
            conn.Open();
            SqlCommand command = new SqlCommand(query, conn);
            //command.Parameters.AddWithValue("@limit",limit);
            using (SqlDataReader reader = command.ExecuteReader()) {               
                while (reader.Read()) {
                    logs.Add(new log((int)reader["id"],(string)reader["log_message"], (DateTime)reader["reg_time"]));
                }  
            }
            conn.Close();
        }
        return logs;
    }
    public void insert_log(string message) {
        string query =  "INSERT INTO Logs (log_message) VALUES ('"+message+"')";
        using (SqlConnection conn = new SqlConnection(_conn_str)) {
            conn.Open();
            SqlCommand command = new SqlCommand(query, conn);
            command.ExecuteNonQuery();
            conn.Close();
        }
    }
}