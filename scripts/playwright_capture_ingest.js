const { chromium } = require('playwright');

(async () => {
  const browser = await chromium.launch({ headless: true, args: ['--no-sandbox','--disable-setuid-sandbox'] });
  const context = await browser.newContext();
  const page = await context.newPage();

  page.on('console', msg => {
    console.log('[page console]', msg.type(), msg.text());
  });

  page.on('request', request => {
    if (request.url().includes('/ingest') && request.method() === 'POST') {
      console.log('[playwright] POST to', request.url());
      console.log('[playwright] postData:', request.postData());
    }
  });

  await page.goto('http://localhost:5250/ingest', { waitUntil: 'load' });
  await page.waitForSelector('#documentText');
  await page.fill('#documentText', 'Playwright captured document text');
  const value = await page.$eval('#documentText', el => el.value);
  console.log('[playwright] textarea value before click:', value);

  await page.click("button[type='submit']");

  // Wait a moment to let requests complete
  await page.waitForTimeout(2000);
  await browser.close();
})();