using System;
using System.Net.Http;
using System.Collections.Generic; 
public class sms
{
    public string _from;
    public string _target;
    public string _text;
    public string _api_key;
    public string _api_sectet;
    public sms(string from, string target, string text) {
        this._from = from;
        this._target = target;
        this._text = text;
        this._api_key = "513e3c1c";
        this._api_sectet = System.Environment.GetEnvironmentVariable("sms_key");
    }
    public async void send(string target = null) {
        if (target == null) {
            target = this._target;
        }
        var values = new Dictionary<string, string>
            {
                { "from", "Guy" },
                { "text", "Hello" },
                { "to", "972544454162" },
                { "api_key", "513e3c1c" },
                { "api_secret", target }
            };
        var content = new FormUrlEncodedContent(values);

        var response = await client.PostAsync("https://rest.nexmo.com/sms/json", content);

        //var responseString = await response.Content.ReadAsStringAsync();
    }
    private static readonly HttpClient client = new HttpClient();
}