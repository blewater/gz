"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
function run(context, myTimer) {
    var timeStamp = new Date().toISOString();
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
// check on which environment the function is running
var environment = process.env.APPSETTING_NODE_ENV || process.env.NODE_ENV;
if (typeof environment === "undefined" || environment === "production") {
    // call the AzureFunction when running in production
    // module.exports = AzureFunction;
    console.log("Production: " + environment);
}
else {
    console.log("Dev mode: " + environment);
}
//# sourceMappingURL=index.js.map