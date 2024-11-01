using System.Collections.ObjectModel;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace SetupTest
{
    //[TestFixture]
    internal class Task2
    {
        private IWebDriver _driver;
        private WebDriverWait _wait;

        //[SetUp]
        public void Setup()
        {
            new DriverManager().SetUpDriver(new ChromeConfig());
            _driver = new ChromeDriver();
            _driver.Manage().Window.Maximize();
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
        }

        private IWebElement GetElement(By locator)
        {
            return _wait.Until(
                SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(locator)
            );
        }

        private ReadOnlyCollection<IWebElement> GetManyElements(By locator)
        {
            return _wait.Until(
                SeleniumExtras.WaitHelpers.ExpectedConditions.PresenceOfAllElementsLocatedBy(
                    locator
                )
            );
        }

        //[Test]
        public void TestWebsite()
        {
            // 1. Atsidaryti tinklalapį
            _driver.Navigate().GoToUrl("https://demowebshop.tricentis.com/");
            Assert.That(_driver.Url.ToLower().Contains("demowebshop.tricentis.com"));

            // 2. Spausti 'Gift Cards' kairiajame meniu.
            var giftCardsLink = GetElement(By.LinkText("Gift Cards"));
            giftCardsLink.Click();
            Assert.That(_driver.Url.ToLower().Contains("demowebshop.tricentis.com/gift-cards"));

            // 3. Pasirinkti prekės, kurios kaina didensė nei 99 nuorodą. Reikia 'neįhardcodinti'
            // pasirenkamos prekės, nes prekių kaina gali keistis ateityje
            var productElement = GetElement(
                By.XPath(
                    "//div[@class='product-item' and .//span[@class='price actual-price'] > 99]"
                )
            );
            var linkElement = productElement.FindElement(By.TagName("a"));
            var link = linkElement.GetAttribute("href");

            linkElement.Click();
            Thread.Sleep(1000);
            Assert.That(_driver.Url.Contains(link));

            // 4. Supildyti laukus 'Recipient's Name:', 'Your Name:' savo nuožiūra
            var recipientsNameField = GetElement(By.ClassName("recipient-name"));
            recipientsNameField.SendKeys("Recipient");

            Assert.That(recipientsNameField.GetAttribute("value") == "Recipient");

            var yourNameField = GetElement(By.ClassName("sender-name"));
            yourNameField.SendKeys("Vardenis");

            Assert.That(yourNameField.GetAttribute("value") == "Vardenis");

            // 5. Į tekstinį lauką 'Qty' įvesti '5000'
            var qtyField = GetElement(By.ClassName("qty-input"));

            qtyField.Clear();
            qtyField.SendKeys("5000");

            Assert.That(qtyField.GetAttribute("value") == "5000");

            // 6. Spausti 'Add to cart' mygtuką
            var addToCartButton = GetElement(By.ClassName("add-to-cart-button"));

            var qtyCartSpan = GetElement(By.ClassName("cart-qty"));

            addToCartButton.Click();

            Thread.Sleep(1000);
            Assert.That(qtyCartSpan.Text == "(5000)");

            // 7. Spausti 'Add to wish list' mygtuką
            var addToWishlistButton = GetElement(By.ClassName("add-to-wishlist-button"));

            var wishilistCartSpan = GetElement(By.ClassName("wishlist-qty"));

            addToWishlistButton.Click();

            Thread.Sleep(1000);
            Assert.That(wishilistCartSpan.Text == "(5000)");

            // 8. Spausti 'Jewelry' kairiajame meniu.
            var jewelryLink = GetElement(By.LinkText("Jewelry"));
            jewelryLink.Click();

            Assert.That(_driver.Url.ToLower().Contains("demowebshop.tricentis.com/jewelry"));

            // 9. Spausti 'Create Your Own Jewelry' nuorodą.
            var ownJewelryLink = GetElement(By.LinkText("Create Your Own Jewelry"));
            ownJewelryLink.Click();

            Assert.That(
                _driver
                    .Url.ToLower()
                    .Contains("demowebshop.tricentis.com/create-it-yourself-jewelry")
            );

            // 10. Pasirinkti reikšmes: 'Material' - 'Silver 1mm', 'Length in cm' - '80', 'Pendant' - 'Star'
            var materialDropdown = GetElement(By.XPath("//div[@class='attributes']//select"));
            var materialSelect = new SelectElement(materialDropdown);
            materialSelect.SelectByText("Silver (1 mm)");

            Assert.That(materialDropdown.GetAttribute("value") == "47");

            var lengthField = GetElement(By.XPath("//div[@class='attributes']//dd/input"));
            lengthField.Clear();
            lengthField.SendKeys("80");

            Assert.That(lengthField.GetAttribute("value") == "80");

            var starPendantRadio = GetElement(
                By.XPath("//li/label[contains(text(), 'Star ')]/preceding-sibling::input")
            );
            starPendantRadio.Click();

            Assert.That(starPendantRadio.GetAttribute("checked") == "true");

            // 11. Į tekstinį lauką 'Qty' įvesti '26'
            qtyField = GetElement(By.ClassName("qty-input"));

            qtyField.Clear();
            qtyField.SendKeys("26");

            Assert.That(qtyField.GetAttribute("value") == "26");

            // 12. Spausti 'Add to cart' mygtuką
            addToCartButton = GetElement(By.ClassName("add-to-cart-button"));
            qtyCartSpan = GetElement(By.ClassName("cart-qty"));
            addToCartButton.Click();

            Thread.Sleep(1000);
            Assert.That(qtyCartSpan.Text == "(5026)");

            // 13. Spausti 'Add to wish list' mygtuką
            addToWishlistButton = GetElement(By.ClassName("add-to-wishlist-button"));
            wishilistCartSpan = GetElement(By.ClassName("wishlist-qty"));
            addToWishlistButton.Click();

            Thread.Sleep(1000);
            Assert.That(wishilistCartSpan.Text == "(5026)");

            // 14. Spausti nuorodą 'Wishlist' puslapio viršuje
            var wishlistLink = GetElement(By.LinkText("Wishlist"));
            wishlistLink.Click();

            Assert.That(_driver.Url.ToLower().Contains("demowebshop.tricentis.com/wishlist"));

            // 15. Abejom prekėm paspausti 'Add to cart' varneles
            var wishlistElements = GetManyElements(By.ClassName("cart-item-row"));

            foreach (var wishlistElement in wishlistElements)
            {
                var addToCartCheck = wishlistElement.FindElement(
                    By.XPath(".//td[@class='add-to-cart']/input")
                );
                addToCartCheck.Click();

                Assert.That(addToCartCheck.GetAttribute("checked") == "true");
            }

            // 16. Spausti 'Add to cart' mygtuką
            addToCartButton = GetElement(By.Name("addtocartbutton"));
            addToCartButton.Click();

            Assert.That(_driver.Url.ToLower().Contains("demowebshop.tricentis.com/cart"));

            // 17. Atsiradus Shopping cart puslapyje patvirtinti, kad 'Sub-Total' reikšmė yra '1002600.00'
            var subTotalElement = GetElement(
                By.XPath(
                    "//td[preceding-sibling::td//span[contains(text(), 'Sub-Total:')]]//span[@class='product-price']"
                )
            );

            Assert.That(subTotalElement.Text == "1002600.00");

            _driver.Dispose();
        }

        //[TearDown]
        public void Teardown()
        {
            _driver.Quit();
            _driver.Dispose();
        }
    }
}
