"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
function run(context, myTimer) {
    var timeStamp = new Date().toISOString();
    var sg = require("@sendgrid/mail");
    sg.setApiKey(process.env.SENDGRID_API_KEY);
    sg.setSubstitutionWrappers("%", "%");
    var msg = {
        to: "salem8@gmail.com",
        from: "help@greenzorro.com",
        templateId: "e056156b-912a-42ac-87c3-7848383a917f",
        substitutions: {
            subject: "Sending with SendGrid Templates is Fun",
            firstname: "Joe",
            email: "joe@mymail.com",
            lastloggedin: timeStamp
        },
    };
    sg.send(msg);
    var sql = require("mssql");
    //     const message = {
    //         "personalizations": [ { "to": [ { "email": "sample@sample.com" } ] } ],
    //        from: { email: "sender@contoso.com" },
    //        subject: "Azure news",
    //        content: [{
    //            type: 'text/plain',
    //            value: input
    //        }]
    //    };
    if (myTimer.isPastDue) {
        context.log("TypeScript is running late!");
    }
    context.log("IdlenessEmailTS function ran! " + timeStamp);
    context.log(GetEnvironmentVariable("AzureWebJobsStorage"));
    context.done();
}
exports.run = run;
function GetEnvironmentVariable(name) {
    return name + ": " + process.env[name];
}
function getDbConfig() {
    var config = {
        server: "***",
        database: "***",
        user: "***",
        password: "***",
        port: 1433,
        // since we're on Windows Azure, we need to set the following options
        options: {
            encrypt: true
        },
        multipleStatements: true,
        parseJSON: true
    };
    return config;
}
function getEnv() {
    // check on which environment the function is running
    var environment = process.env.APPSETTING_NODE_ENV || process.env.NODE_ENV;
    if (typeof environment === "undefined") {
        environment = "production";
    }
    if (environment === "production") {
        // call the AzureFunction when running in production
        // module.exports = AzureFunction;
        console.log("Production: " + environment);
    }
    else {
        console.log("Dev mode: " + environment);
    }
    return environment;
}
//# sourceMappingURL=index.js.map