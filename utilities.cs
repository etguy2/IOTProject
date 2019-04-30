
using System;    
using System.Text;  
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Data.SqlClient;

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
}
