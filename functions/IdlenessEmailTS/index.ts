export function run(context: any, myTimer: any): any {
    const timeStamp: string = new Date().toISOString();

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

// check on which environment the function is running
const environment = process.env.APPSETTING_NODE_ENV || process.env.NODE_ENV;
if (typeof environment === "undefined" || environment === "production") {
    // call the AzureFunction when running in production
    // module.exports = AzureFunction;
    console.log(`Production: ${environment}`);
} else {
    console.log(`Dev mode: ${environment}`);

}
