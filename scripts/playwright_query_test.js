const { chromium } = require('playwright');

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  await page.goto('http://localhost:5250');
  // click Query link or button
  await page.click('text=Query');
  await page.fill('input#queryText', 'what is the capital of France?');
  await page.click('button:has-text("Query")');
  // wait for response or message
  await page.waitForTimeout(2000);
  console.log('Done');
  await browser.close();
})();