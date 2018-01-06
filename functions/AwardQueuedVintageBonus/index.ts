import * as puppeteer from "puppeteer";

interface IBonusReq {
    userId: number;
    amount: number;
    currency: string;
    comment: string;
}

export function run(context: any, bonusReq: IBonusReq) {
    context.log(`TypeScript queue trigger function processed work item: ${bonusReq}`);
    context.log(`userId: ${bonusReq.userId}, amount: ${bonusReq.amount}, currency: ${bonusReq.currency}, comment: ${bonusReq.comment}`);
    browse();
    context.done();
}

async function browse() {
  const browser = await puppeteer.launch();
  const page = await browser.newPage();

  const everyMatrixBOUrl = "https://admin3.gammatrix.com/Admin/Default.aspx";

  await page.goto(everyMatrixBOUrl);
  // await page.screenshot({ path: 'screenshots/github.png' });

  await browser.close();
}