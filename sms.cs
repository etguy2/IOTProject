using System;
using System.Net.Http;
using System.Collections.Generic; 
public class sms
{
    private string _from;
    private string _target;
    private string _text;
    private string _api_key;
    private string _api_sectet;
    public string _last_response;
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
                { "from", this._from },
                { "text", this._text },
                { "to", target },
                { "api_key", this._api_key },
                { "api_secret", this._api_sectet }
            };
        var content = new FormUrlEncodedContent(values);

        var response = await client.PostAsync("https://rest.nexmo.com/sms/json", content);

        this._last_response = await response.Content.ReadAsStringAsync();
    }
    private static readonly HttpClient client = new HttpClient();
}