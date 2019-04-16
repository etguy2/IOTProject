using Newtonsoft.Json;  
public class response
{
    public int status;
    public string description;
    public response() {}
    public response(int status, string description) {
        this.setData(status, description);
    }
    public void setData(int status, string description) {
        this.status = status;
        this.description = description;
    }
}