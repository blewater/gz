"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const puppeteer = require("puppeteer");
let logger;
function failureCallback(error) {
    logger("--> Error = '" + error + "'");
}
function run(context, bonusReq) {
    logger = context.log;
    context.log(`TypeScript queue trigger function processed work item: ${bonusReq}`);
    context.log(`userId: ${bonusReq.userId}, amount: ${bonusReq.amount}, currency: ${bonusReq.currency}, comment: ${bonusReq.comment}`);
    context.log("Node.js queue trigger function processed work item", context.bindings.gzWithdrawnVintagesQueueBonus);
    context.log("queueTrigger =", context.bindingData.queueTrigger);
    context.log("expirationTime =", context.bindingData.expirationTime);
    context.log("insertionTime =", context.bindingData.insertionTime);
    context.log("nextVisibleTime =", context.bindingData.nextVisibleTime);
    context.log("id =", context.bindingData.id);
    context.log("popReceipt =", context.bindingData.popReceipt);
    context.log("dequeueCount =", context.bindingData.dequeueCount);
    context.done();
}
exports.run = run;
function browse(logger) {
    const everyMatrixBOUrl = "https://admin3.gammatrix.com/Admin/Default.aspx";
    let browser;
    puppeteer
        .launch({
        headless: true,
        args: ["--no-sandbox", "--single-process", "--disable-gpu"]
    })
        .then(b => {
        browser = b;
        return browser.newPage();
    }, failureCallback)
        .then(page => {
        return page.goto(everyMatrixBOUrl);
    }, failureCallback)
        .then(r => {
        browser.close();
    }, failureCallback);
}
//# sourceMappingURL=index.js.map