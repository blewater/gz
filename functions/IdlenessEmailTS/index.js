"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var sg = require("@sendgrid/mail/src/mail");
function run(context, myTimer) {
    var timeStamp = new Date().toISOString();
    var apiKey = GetEnvironmentVariable("SENDGRID_API_KEY");
    var sendGridGroupId = GetEnvironmentNumVariable("IDLENESS_GROUP_ID");
    sg.setApiKey(apiKey);
    sg.setSubstitutionWrappers("%", "%");
    sg.send({
        to: "salem8@gmail.com",
        from: "mario.karagiorgas@greenzorro.com",
        templateId: "e056156b-912a-42ac-87c3-7848383a917f",
        substitutions: {
            subject: "Sending with SendGrid Templates is Fun",
            firstname: "Joe",
            email: "joe@mymail.com",
            lastloggedin: timeStamp
        },
        categories: ["Transactional", "Idleness"],
        asm: {
            groupId: sendGridGroupId
        }
    })
        .then(function () {
        return console.log("Mail sent successfully to ");
    })
        .then(function () {
        return context.log("Mail sent successfully to ");
    })
        .catch(function (error) {
        return console.error(error.toString());
    });
    var sql = require("mssql");
    if (myTimer.isPastDue) {
        context.log("TypeScript is running late!");
    }
    context.log("IdlenessEmailTS function ran! " + timeStamp);
    context.log(GetNamedEnvironmentVariable("AzureWebJobsStorage"));
    context.done();
}
exports.run = run;
function GetEnvironmentNumVariable(name) {
    if (typeof name === "undefined") {
        return 0;
    }
    var configValue = process.env[name];
    if (typeof configValue === "undefined") {
        return 0;
    }
    return Number.parseInt(configValue);
}
function GetEnvironmentVariable(name) {
    if (typeof name === "undefined") {
        return "";
    }
    var configValue = process.env[name];
    if (typeof configValue === "undefined") {
        configValue = "";
    }
    return configValue;
}
function GetNamedEnvironmentVariable(name) {
    var configValue = GetEnvironmentVariable(name);
    if (configValue.length === 0) {
        configValue = "Not found";
    }
    else {
        configValue = name + ": " + process.env[name];
    }
    return configValue;
}
function getDbConfig() {
    var config = {
        server: "***",
        database: "***",
        user: "***",
        password: "***",
        port: 1433,
        options: {
            encrypt: true
        },
        multipleStatements: true,
        parseJSON: true
    };
    return config;
}
function getEnv() {
    var environment = process.env.APPSETTING_NODE_ENV || process.env.NODE_ENV;
    if (typeof environment === "undefined") {
        environment = "production";
    }
    if (environment === "production") {
        console.log("Production: " + environment);
    }
    else {
        console.log("Dev mode: " + environment);
    }
    return environment;
}
//# sourceMappingURL=index.js.map