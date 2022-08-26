const webdriver = require('selenium-webdriver'),
    { By, until, Key } = webdriver;

// 10 bots.
for (let i = 1; i <= 10; i++) {
    const driver = new webdriver.Builder()
        .forBrowser('chrome')
        .usingServer('http://host.docker.internal:4444/wd/hub')
        .build();

    try {
        driver.manage().deleteAllCookies();

        driver.get('http://host.docker.internal:30080?bot=true').then(async () => {
            console.log('connected');
            try {
                try {
                    await driver.wait(
                        until.elementLocated(By.className('statistics-entry')),
                        2000
                    );

                    driver.executeScript(`
const input = document.createElement("input");
input.id = 'input';
document.body.insertBefore(input, document.body.firstChild);
`);

                    while (true) {
                        await new Promise(x => { setTimeout(x, 1000); });
                        console.log('sending ENTER');
                        const input = await driver.findElement(By.id('input'));
                        await input.sendKeys('webdriver', Key.RETURN);
                        console.log('sent ENTER');

                        let cursor = await driver.wait(
                            until.elementLocated(By.className('cursor')),
                            2000
                        );

                        while (true) {
                            try {
                                cursor = await driver.findElement(By.className('cursor'));
                                let currentLetterValue = await cursor.getText();
                                if (currentLetterValue === '') currentLetterValue = ' ';
                                console.log('cursor', currentLetterValue);
                                await input.sendKeys(currentLetterValue);

                                await delayFor(120);
                            } catch (err) {
                                console.log(err);
                                break;
                            }

                            try {
                                await driver.findElement(By.className('typed-error'));
                            } catch {
                                continue;
                            }

                            throw new Error('Found errors!');
                        }
                    }
                } catch (err) {
                    console.log(err);
                    console.log('did not get the element');
                }
            } finally {
                driver.quit();
            }
        });
    } catch (err) {
        console.error('error connecting', err);
        driver.quit();
    }
}

function delayFor(speedWpm) {
    return new Promise(x => { setTimeout(x, calculateDelay(speedWpm)); });
}

function calculateDelay(speedWpm) {
    const speedCpm = speedWpm * 5;
    const charactersPerSecond = speedCpm / 60;
    const secondsPerCharacter = 1 / charactersPerSecond;
    const msPerCharacter = secondsPerCharacter * 1000;

    return msPerCharacter;
}
