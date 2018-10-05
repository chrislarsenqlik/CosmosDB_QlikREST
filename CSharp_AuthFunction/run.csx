#r "Newtonsoft.Json"

using System.Net;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{
    //JSON input example
    //{
    // "verb": "GET",
    // "resourceType": "docs",
    // "resourceId": "dbs/video_gamesdb/colls/vgdata",
    // "masterKey": "MASTER HERE"
    //}
    
    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
    dynamic data;
    try{
        data = JsonConvert.DeserializeObject(requestBody);
    }catch (Exception e){
        return new BadRequestObjectResult("JSON is not in correct format");
    }
    
    string verb = data.verb;
    string type = data.resourceType;
    string link = data.resourceId;
    string key = data.masterKey;
    string keytype = "master";
    string keyver = "1.0";

    string datetime = DateTime.UtcNow.ToString("R");

    
    if (!string.IsNullOrEmpty(verb) && !string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(link) && !string.IsNullOrEmpty(key))
    {
        string token ="";
        try{

            token = GenerateAuthToken(verb,type,link,datetime,key,keytype,keyver, log);

        }catch (Exception e){

            return new BadRequestObjectResult(e.Message);

        }
        return (ActionResult)new OkObjectResult( $"{{\"sessionId\": \"{token}\", \"date\": \"{datetime}\" }}");
    } else {

        return new BadRequestObjectResult("Please pass all params: verb, resourceType, resourceId and masterKey in the request body");

    }
}


static string GenerateAuthToken(string verb, string resourceType, string resourceId, string date, string key, string keyType, string tokenVersion,ILogger log )
{   
    var hmacSha256 = new System.Security.Cryptography.HMACSHA256 { Key = Convert.FromBase64String(key) };

    verb = verb ?? "";
    resourceType = resourceType ?? "";
    resourceId = resourceId ?? "";

    string payLoad = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}\n{1}\n{2}\n{3}\n{4}\n",
        verb.ToLowerInvariant(),
        resourceType.ToLowerInvariant(),
        resourceId,
        date.ToLowerInvariant(),
        ""
    );

    byte[] hashPayLoad = hmacSha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payLoad));
    string signature = Convert.ToBase64String(hashPayLoad);

   
    return System.Web.HttpUtility.UrlEncode(String.Format(System.Globalization.CultureInfo.InvariantCulture, "type={0}&ver={1}&sig={2}",
        keyType,
        tokenVersion,
        signature));
}