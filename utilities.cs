
using System;    
using System.Text;  
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Data.SqlClient;
using CarSharing.Cryptography;

public static class utilitles {
    private static Random random = new Random();
    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
        .Select(s => s[random.Next(s.Length)]).ToArray());
    }
    public static bool isKeyExist(SqlDataReader reader, string columnName) {
        for (int i=0; i < reader.FieldCount; i++)
        {
            
            if (reader.GetName(i).Equals(columnName,StringComparison.InvariantCultureIgnoreCase))
                return true;
        }
        return false; 
    }

    public static string safeCast(SqlDataReader reader, string slot_name) {
        return (utilitles.isKeyExist(reader, slot_name) && reader[slot_name] != DBNull.Value) ? (string)reader[slot_name] : string.Empty;
    }
    public static Image CropImage(Image img) {
        int x = img.Width/2;
        int y = img.Height/2;
        int r = Math.Min(x, y);
        Bitmap tmp = null;
        tmp = new Bitmap(2*r, 2*r);
        using (Graphics g = Graphics.FromImage(tmp)) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TranslateTransform(tmp.Width / 2, tmp.Height / 2);
            GraphicsPath gp = new GraphicsPath();
            gp.AddEllipse(0-r, 0-r, 2*r, 2*r);
            Region rg = new Region(gp);
            g.SetClip(rg, CombineMode.Replace);
            Bitmap bmp = new Bitmap(img);
            g.DrawImage(bmp, new Rectangle(-r, -r, 2*r, 2*r), new Rectangle(x-y, y-r, 2*r, 2*r), GraphicsUnit.Pixel);
        }
        return tmp;
    }

    public static bool validateUser(int user_id, string login_hash) {
        string validate_query = "SELECT password_enc, enc_string FROM Users WHERE id = @user_id";
        string _conn_str = System.Environment.GetEnvironmentVariable("sqldb_connection");
        bool status = false;
        using (SqlConnection conn = new SqlConnection(_conn_str)) {
            conn.Open();
            SqlCommand command = new SqlCommand(validate_query, conn);
            command.Parameters.AddWithValue("@user_id", user_id);
            using (SqlDataReader reader = command.ExecuteReader()) {               
                if (reader.Read()) {
                    string stored_login_hash = SHA.GenerateSHA256String((string)reader["password_enc"]+(string)reader["enc_string"]).ToLower();
                    if (stored_login_hash == login_hash)
                        status = true;
                }
            }
            conn.Close();
        }
        return status;
    }
}
