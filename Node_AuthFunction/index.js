var crypto = require("crypto");  

module.exports = async function (context, req) {
    context.log('Creating Cosmos DB auth token.');

    var verb = req.body.verb;
    var resourceType = req.body.resourceType;
    var resourceId = req.body.resourceId;
    var date = new Date().toUTCString().toLowerCase();
    var key = req.body.masterKey;
    
    var masterKey = new Buffer(key, "base64"); 

    var text = (verb || "").toLowerCase() + "\n" +   
               (resourceType || "").toLowerCase() + "\n" +   
               (resourceId || "") + "\n" +   
               date + "\n" +   
               "" + "\n";

    var body = new Buffer(text, "utf8");  
    var signature = crypto.createHmac("sha256", masterKey).update(body).digest("base64");  

    var MasterToken = "master";  
    var TokenVersion = "1.0";  
    var authToken = encodeURIComponent("type=" + MasterToken + "&ver=" + TokenVersion + "&sig=" + signature);

    context.res = {
        status: 200,
        body: { "sessionId": authToken, "date": date }
    };
};