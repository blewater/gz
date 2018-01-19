import { JSDOM, CookieJar, DOMWindow } from "jsdom";
import { CookieJar as ToughCookieJar, MemoryCookieStore } from "tough-cookie";

import request = require("request");
import { RequestCallback } from "request";
import { error } from "util";

const everyMatrixBOUrl = "https://admin3.gammatrix.com/Admin/Login.aspx";
let u = "presto";
let p = "gz2017!@";
let t = "3DFEC757D808494";

let jar = request.jar(); // stores cookies in the “jar”, we can re-use them in future authenticated requests

let options : request.CoreOptions = {
    headers: [
      {
        name: "content-type",
        value: "application/x-www-form-urlencoded"
      }
    ],
    jar: jar,
    followAllRedirects: true,
    form: {
        "rtbTop_text" : u,
        "rtbTop" : u,
        "rtbMid_text": p,
        "rtbMid": p,
        "rtbBottom_text": "3DFEC757D808494",
        "rtbBottom": "3DFEC757D808494"
    }
};

let loggedIn : RequestCallback = function(error : any, response : request.RequestResponse, body : any) {
    if (error) {
        console.error(error);
        return;
    }
    var window = JSDOM.fromURL(response.url)
    let $ : DOMWindow;
    JSDOM.fromURL("http://code.jquery.com/jquery.js")
    .then(function(script: JSDOM) {
        $ = script.window;
    });

};

request.post(everyMatrixBOUrl, options, loggedIn);

const PhantomApiKey = "ak-kegrv-y96t7-4pssy-k90d2-6ct4g";

interface IBonusReq {
    userId: number;
    amount: number;
    currency: string;
    comment: string;
}

let logger;
    function failureCallback(error : string) {
        logger("--> Error = '" + error + "'");
    }

export function run(context: any, bonusReq: IBonusReq) {
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
    // browse(context.log);
    context.done();
}

function browse(logger: any) {
    // const browser = await puppeteer.launch();
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
        // .then(response => {
        //     return page.screenshot({path: screenshotPath, fullPage: true});
        // }, failureCallback)
        .then(r => {
            browser.close();
        }, failureCallback);

    //   const page = await browser.newPage();

    // page.goto(everyMatrixBOUrl);

    // await page.goto(everyMatrixBOUrl);
    // await page.screenshot({ path: 'screenshots/github.png' });

    // await browser.close();
}