import sg = require("@sendgrid/mail/src/mail");

export function run(context: any, myTimer: any): any {
    const timeStamp: string = new Date().toISOString();
    const apiKey:string = GetEnvironmentVariable("SENDGRID_API_KEY");
    const sendGridGroupId:number = GetEnvironmentNumVariable("IDLENESS_GROUP_ID");

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
    .then(() =>
        console.log("Mail sent successfully to ")
    )
    .then(() =>
        context.log("Mail sent successfully to ")
    )
    .catch(error =>
        console.error(error.toString()
    ));

    const sql = require("mssql");

    if(myTimer.isPastDue) {
        context.log(`TypeScript is running late!`);
    }
    context.log(`IdlenessEmailTS function ran! ${timeStamp}`);
    context.log(GetNamedEnvironmentVariable("AzureWebJobsStorage"));

    context.done();
}

function GetEnvironmentNumVariable(name: string | undefined) : number {
    if (typeof name === "undefined") {
        return 0;
    }
    let configValue = process.env[name];

    if (typeof configValue === "undefined") {
        return 0;
    }

    return Number.parseInt(configValue);
}
function GetEnvironmentVariable(name: string | undefined) : string {
    if (typeof name === "undefined") {
        return "";
    }
    let configValue = process.env[name];

    if (typeof configValue === "undefined") {
        configValue = "";
    }

    return configValue;
}
function GetNamedEnvironmentVariable(name: string) : string {
    let configValue = GetEnvironmentVariable(name);

    if (configValue.length === 0) {
        configValue = "Not found";
    } else {
        configValue = name + ": " + process.env[name];
    }
    return configValue;
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
