import * as SendGrid from "sendgrid";

export class SendGridMail extends SendGrid.mail.Mail {}

export function run(context: any, myTimer: any): any {
    const timeStamp: string = new Date().toISOString();
    const sg = require("@sendgrid/mail");
    sg.setApiKey(process.env.SENDGRID_API_KEY);
    sg.setSubstitutionWrappers("%", "%");
    const msg = {
        to: "salem8@gmail.com",
        from: "help@greenzorro.com",
        templateId: "e056156b-912a-42ac-87c3-7848383a917f",
        substitutions: {
            subject: "Sending with SendGrid Templates is Fun",
            firstname: "Joe",
            email: "joe@mymail.com",
            lastloggedin: timeStamp
        },
        categories: ["Transactional", "Idleness"],
        batchId: "e056156b-912a-42ac-87c3-7848383a917f" + "_20171017",
        asm: {
            groupId: 5325
        }
    };
    sg
    .send(msg)
    .then(() => console.log("Mail sent successfully"))
    .catch(error => console.error(error.toString()));

    const sql = require("mssql");

//     const message = {
//         "personalizations": [ { "to": [ { "email": "sample@sample.com" } ] } ],
//        from: { email: "sender@contoso.com" },
//        subject: "Azure news",
//        content: [{
//            type: 'text/plain',
//            value: input
//        }]
//    };

    if(myTimer.isPastDue) {
        context.log(`TypeScript is running late!`);
    }
    context.log(`IdlenessEmailTS function ran! ${timeStamp}`);
    context.log(GetEnvironmentVariable("AzureWebJobsStorage"));

    context.done();
}

function GetEnvironmentVariable(name: string) {
    return name + ": " + process.env[name];
}

function getDbConfig() {

    const config = {
        server: "***", // use your SQL server name
        database: "***", // database to connect to
        user: "***", // use your username
        password: "***", // use your password
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

function getEnv() : string {
    // check on which environment the function is running
    let environment = process.env.APPSETTING_NODE_ENV || process.env.NODE_ENV;
    if (typeof environment === "undefined") {
        environment = "production";
    }
    if (environment === "production") {
        // call the AzureFunction when running in production
        // module.exports = AzureFunction;
        console.log(`Production: ${environment}`);
    } else {
        console.log(`Dev mode: ${environment}`);

    }
    return environment;
}
