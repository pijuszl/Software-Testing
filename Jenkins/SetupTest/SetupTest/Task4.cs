using System.Reflection;
using System.Text;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace SetupTest
{
    [TestFixture]
    public class Task4
    {
        private IWebDriver _driver;
        private WebDriverWait _wait;

        private string _firstName = "Vardenis";
        private string _lastName = "Pavardenis";
        private string _emailDomain = "@gmail.com";
        private string? _email;
        private string _password = "pass123";

        private IWebElement FindElement(By locator)
        {
            return _wait.Until(ExpectedConditions.ElementIsVisible(locator));
        }

        private static string GenerateRandomString(int length)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyz1234567890";
            StringBuilder sb = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                sb.Append(validChars[rnd.Next(validChars.Length)]);
            }
            return sb.ToString();
        }

        [OneTimeSetUp]
        public void CreateUser()
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyPath = Path.GetDirectoryName(assemblyLocation);
            var driverPath = assemblyPath;
            var service = ChromeDriverService.CreateDefaultService(driverPath);
            var options = new ChromeOptions();
            _driver = new ChromeDriver(service, options);
            _driver.Manage().Window.Maximize();

            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));

            bool userCreated = false;

            while (!userCreated)
            {
                string randomPart = GenerateRandomString(10);
                string emailToTry = $"{randomPart}{_emailDomain}";

                _driver.Navigate().GoToUrl("https://demowebshop.tricentis.com/");
                FindElement(By.LinkText("Log in")).Click();
                FindElement(By.XPath("//input[@value='Register']")).Click();
                FindElement(By.Id("gender-male")).Click();
                FindElement(By.Id("FirstName")).SendKeys(_firstName);
                FindElement(By.Id("LastName")).SendKeys(_lastName);

                FindElement(By.Id("Email")).SendKeys(emailToTry);
                FindElement(By.Id("Password")).SendKeys(_password);
                FindElement(By.Id("ConfirmPassword")).SendKeys(_password);
                FindElement(By.Id("register-button")).Click();

                var errorMessages = _driver.FindElements(
                    By.CssSelector(".validation-summary-errors li")
                );
                if (
                    errorMessages.Count > 0
                    && errorMessages[0].Text.Contains("The specified email already exists")
                )
                {
                    _driver.Navigate().Refresh();
                }
                else
                {
                    userCreated = true;
                    _email = emailToTry;
                }
            }

            _driver.Quit();
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            _driver.Dispose();
        }

        [SetUp]
        public void Setup()
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyPath = Path.GetDirectoryName(assemblyLocation);
            var driverPath = assemblyPath;
            var service = ChromeDriverService.CreateDefaultService(driverPath);
            var options = new ChromeOptions();

            _driver = new ChromeDriver(service, options);
            _driver.Manage().Window.Maximize();

            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
        }

        [TearDown]
        public void Teardown()
        {
            _driver.Dispose();
        }

        /*        [Test]
                public void TestData1()
                {
                    LoginUser();
        
                    FindElement(By.LinkText("Digital downloads")).Click();
        
                    AddItemsToBasket("data1.txt");
                    CompleteCheckout();
                }*/

        [Test]
        [TestCase("data1.txt")]
        [TestCase("data2.txt")]
        public void TestData2(String fileName)
        {
            LoginUser();

            FindElement(By.LinkText("Digital downloads")).Click();

            AddItemsToBasket(fileName);
            CompleteCheckout();
        }

        private void LoginUser()
        {
            _driver.Navigate().GoToUrl("https://demowebshop.tricentis.com/");
            FindElement(By.LinkText("Log in")).Click();
            FindElement(By.Id("Email")).SendKeys(_email);
            FindElement(By.Id("Password")).SendKeys(_password);
            FindElement(By.XPath("//input[@value='Log in']")).Click();
        }

        private void AddItemsToBasket(string fileName)
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyPath = Path.GetDirectoryName(assemblyLocation);
            var filePath = Path.Combine(assemblyPath, fileName);

            var items = File.ReadAllLines(filePath);

            foreach (var item in items)
            {
                FindElement(
                        By.XPath(
                            $"//div[@class='product-item' and .//a[text() = '{item}']]//input[@value='Add to cart']"
                        )
                    )
                    .Click();
                Thread.Sleep(1000);
            }
        }

        private void CompleteCheckout()
        {
            FindElement(By.LinkText("Shopping cart")).Click();
            FindElement(By.Id("termsofservice")).Click();
            FindElement(By.Id("checkout")).Click();

            FillForm();

            FindElement(By.XPath("//input[@class='button-1 new-address-next-step-button']"))
                .Click();
            FindElement(By.XPath("//input[@class='button-1 payment-method-next-step-button']"))
                .Click();
            FindElement(By.XPath("//input[@class='button-1 payment-info-next-step-button']"))
                .Click();

            var confirmButton = FindElement(
                By.XPath("//input[@class='button-1 confirm-order-next-step-button']")
            );
            ((IJavaScriptExecutor)_driver).ExecuteScript(
                "arguments[0].scrollIntoView(true);",
                confirmButton
            );

            confirmButton.Click();

            var confirmation = FindElement(By.XPath("//div[@class='title']/strong"));
            Assert.That(confirmation.Text.Equals("Your order has been successfully processed!"));
        }

        private void FillForm()
        {
            var addressSelectExists =
                _driver.FindElements(By.Id("billing-address-select")).Count > 0;

            if (addressSelectExists)
            {
                var billingAddressSelect = FindElement(By.Id("billing-address-select"));
                var selectElement = new SelectElement(billingAddressSelect);

                if (
                    selectElement.Options.Any(
                        option =>
                            option.Text.Contains(_firstName) && option.Text.Contains(_lastName)
                    )
                )
                {
                    selectElement.SelectByIndex(0);
                }
            }

            if (
                !addressSelectExists
                || FindElement(By.Id("billing-address-select")).GetAttribute("value") == ""
            )
            {
                var countryDropdown = FindElement(By.Id("BillingNewAddress_CountryId"));
                var selectCountry = new SelectElement(countryDropdown);
                selectCountry.SelectByValue("156");

                FindElement(By.Id("BillingNewAddress_City")).SendKeys("Vilnius");
                FindElement(By.Id("BillingNewAddress_Address1")).SendKeys("Didlaukio g. 47");
                FindElement(By.Id("BillingNewAddress_ZipPostalCode")).SendKeys("LT-08303");
                FindElement(By.Id("BillingNewAddress_PhoneNumber")).SendKeys("+37060650555");
            }
        }
    }
}
