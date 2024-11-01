using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SetupTest
{
    [TestFixture]
    internal class Task3
    {
        private IWebDriver _driver;
        private WebDriverWait _wait;

        [SetUp]
        public void Setup()
        {
            new DriverManager().SetUpDriver(new ChromeConfig());
            _driver = new ChromeDriver();
            _driver.Manage().Cookies.DeleteAllCookies();
            _driver.Manage().Window.Maximize();

            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
        }

        private IWebElement FindElement(By locator)
        {
            return _wait.Until(ExpectedConditions.ElementIsVisible(locator));
        }

        [Test]
        public void FirstTest()
        {
            // 1. Atsidaryti tinklalapį
            _driver.Navigate().GoToUrl("https://demoqa.com/");
            Assert.That(_driver.Url.ToLower().Contains("demoqa.com"));

            //2. Uždaryti Cookies sutikimo langą.
            FindElement(By.XPath("//button[./p[contains(text(), 'Consent')]]")).Click();
            //Thread.Sleep(2000);

            //3. Pasirinkti "Widgets" kortelę
            FindElement(
                    By.XPath(
                        "//div[@class='category-cards']/div[.//h5[contains(text(), 'Widgets')]]"
                    )
                )
                .Click();
            Assert.That(_driver.Url.ToLower().Contains("demoqa.com/widgets"));

            //4. Pasirinkti meniu punktą "Progress Bar"
            FindElement(By.XPath("//li[./span[contains(text(), 'Progress Bar')]]")).Click();
            Assert.That(_driver.Url.ToLower().Contains("demoqa.com/progress-bar"));

            //5. Spausti mygtuką "Start"
            FindElement(By.Id("startStopButton")).Click();

            // 6. Sulaukti kol bus 100% ir paspausti "Reset"
            Func<IWebDriver, bool> waitForProgressBar = driver =>
            {
                var progressValue = driver.FindElement(By.XPath("//div[@role='progressbar']")).Text;
                return progressValue.Equals("100%");
            };
            _wait.Until(waitForProgressBar);

            FindElement(By.Id("resetButton")).Click();

            // 7. Įsitikinti, kad progreso eilutė tuščia (0%).
            var progressElement = FindElement(By.XPath("//div[@role='progressbar']")).Text;
            Assert.That(progressElement.Contains("0%"));

            _driver.Quit();
        }

        [Test]
        public void SecondTest()
        {
            // 1. Atsidaryti https://demoqa.com/
            _driver.Navigate().GoToUrl("https://demoqa.com/");
            Assert.That(_driver.Url.ToLower().Contains("demoqa.com"));

            // 2. Uždaryti Cookies sutikimo langą.
            FindElement(By.XPath("//button[./p[contains(text(), 'Consent')]]")).Click();

            //3. Pasirinkti "Widgets" kortelę
            FindElement(
                    By.XPath(
                        "//div[@class='category-cards']/div[.//h5[contains(text(), 'Elements')]]"
                    )
                )
                .Click();
            Assert.That(_driver.Url.ToLower().Contains("demoqa.com/elements"));

            // 4. Pasirinkti meniu punktą "Web Tables"
            FindElement(By.XPath("//li[./span[contains(text(), 'Web Tables')]]")).Click();
            Assert.That(_driver.Url.ToLower().Contains("demoqa.com/webtables"));

            // 5. Pridėti pakankamai elementų, kad atsirastų antras puslapis puslapiavime
            var nextPageButtonLocator = By.XPath("//div[@class='-next']/button");
            FluentWait(nextPageButtonLocator);

            // 6. Pasirinkti antrą puslapį paspaudus "Next"
            var nextPageButton = FindElement(nextPageButtonLocator);
            ((IJavaScriptExecutor)_driver).ExecuteScript(
                "arguments[0].scrollIntoView(true);",
                nextPageButton
            );
            nextPageButton.Click();
            Thread.Sleep(2000);

            // 7. Ištrinti elementą antrajame puslapyje
            FindElement(By.XPath("//span[@title='Delete']")).Click();

            // 8. Įsitikinti, kad automatiškai puslapiavimas perkeliamas
            // į pirmąjį puslapį ir kad puslapių skaičius sumažėjo ligi vieno.
            var pageCount = FindElement(By.XPath("//span[@class='-totalPages']"));
            Assert.That(pageCount.Text.Equals("1"));

            Thread.Sleep(1000);

            _driver.Quit();
        }

        private void FluentWait(By locator)
        {
            var fluentWait = new DefaultWait<IWebDriver>(_driver)
            {
                Timeout = TimeSpan.FromSeconds(30),
                PollingInterval = TimeSpan.FromMilliseconds(500)
            };
            fluentWait.IgnoreExceptionTypes(typeof(NoSuchElementException));

            fluentWait.Until(driver =>
            {
                FindElement(By.Id("addNewRecordButton")).Click();
                FillForm(
                    "Vardenis",
                    "Pavardenis",
                    "vardenis.pavardenis@gmail.com",
                    "30",
                    "10000",
                    "Developer"
                );
                FindElement(By.Id("submit")).Click();

                var nextPageButton = driver.FindElement(locator);
                return nextPageButton.Displayed && nextPageButton.Enabled;
            });
        }

        private void FillForm(
            string firstName,
            string lastName,
            string email,
            string age,
            string salary,
            string department
        )
        {
            FindElement(By.Id("firstName")).SendKeys(firstName);
            FindElement(By.Id("lastName")).SendKeys(lastName);
            FindElement(By.Id("userEmail")).SendKeys(email);
            FindElement(By.Id("age")).SendKeys(age);
            FindElement(By.Id("salary")).SendKeys(salary);
            FindElement(By.Id("department")).SendKeys(department);
        }

        [TearDown]
        public void Teardown()
        {
            _driver.Dispose();
        }
    }
}
