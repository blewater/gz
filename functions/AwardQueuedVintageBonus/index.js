"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
const puppeteer = require("puppeteer");
function run(context, item) {
    context.log(`TypeScript queue trigger function processed work item: ${item}`);
    browse();
    context.done();
}
exports.run = run;
function browse() {
    return __awaiter(this, void 0, void 0, function* () {
        const browser = yield puppeteer.launch();
        const page = yield browser.newPage();
        const everyMatrixBOUrl = "https://admin3.gammatrix.com/Admin/Default.aspx";
        yield page.goto(everyMatrixBOUrl);
        yield browser.close();
    });
}
//# sourceMappingURL=index.js.map