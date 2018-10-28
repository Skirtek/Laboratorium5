using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace SeleniumTests
{
    public class MainTestClass : IDisposable
    {
        public readonly IWebDriver Driver;

        #region CTOR
        public MainTestClass()
        {
            try
            {
                Driver = new ChromeDriver(AppResources.Common_PathToDriver);
                Driver.Manage().Window.Maximize();
                //awaiter to give site time to proceed
                Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{AppResources.Common_InitializeBrowserError} {ex}");
            }
        }
        #endregion

        #region TestMethods
        [Fact]
        public void TryToLogin()
        {
            try
            {
                //Arrange
                Driver.Navigate().GoToUrl(AppResources.JakDojade_Url);

                //Act
                Driver.FindElement(By.ClassName("cmp-intro_acceptAll")).Click();
                Driver.FindElement(By.ClassName("cn-login-text")).Click();

                var loginField = Driver.FindElement(By.Id("cn-login-name"));
                loginField.Click();
                loginField.SendKeys(AppResources.JakDojade_TestLogin);
                var passwordField = Driver.FindElement(By.Id("cn-login-password"));
                passwordField.Click();
                passwordField.SendKeys(AppResources.JakDojade_TestPassword);

                //await for button to change state
                WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
                wait.Until(drv => drv.FindElement(By.ClassName("cn-login-submit")).Enabled);

                Driver.FindElement(By.ClassName("cn-login-submit")).Click();

                bool checkIfLoggedIn = CheckIfElementHasSpecifiedText("div.cn-login-text.ng-binding", AppResources.JakDojade_TestLogin);

                Driver.FindElement(By.CssSelector("div.cn-login-text.ng-binding")).Click();
                Driver.FindElement(By.ClassName("btn-link_logout")).Click();

                //Assert
                Assert.True(checkIfLoggedIn);
                Assert.True(CheckIfElementHasSpecifiedText("div.cn-login-text.ng-scope", AppResources.JakDojade_TextToAssert));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        [Fact]
        public void OpenFurtherWikipediaSites()
        {
            //Arrange
            Driver.Navigate().GoToUrl(AppResources.Wikipedia_Url);

            //Act
            var searchInput = Driver.FindElement(By.Id("searchInput"));
            searchInput.Click();
            searchInput.SendKeys(AppResources.Wikipedia_FirstSearch);
            Driver.FindElement(By.Id("searchButton")).Click();
            Driver.FindElement(By.LinkText(AppResources.Wikipedia_SecondSearch)).Click();
            Driver.FindElement(By.LinkText(AppResources.Wikipedia_ThirdSearch)).Click();
            Driver.FindElement(By.LinkText(AppResources.Wikipedia_FourthSearch)).Click();
            Driver.FindElement(By.CssSelector("p > a[title='Teodozjusz I Wielki']")).Click();
            Driver.FindElement(By.LinkText(AppResources.Wikipedia_SixthSearch)).Click();
            Driver.FindElement(By.LinkText(AppResources.Wikipedia_SeventhSearch)).Click();
            Driver.FindElement(By.LinkText(AppResources.Wikipedia_EighthSearch)).Click();

            var js = (IJavaScriptExecutor)Driver;
            js.ExecuteScript("window.scrollTo(0,742)");
            Driver.FindElement(By.LinkText(AppResources.Wikipedia_EndSearch)).Click();

            //Assert
            Assert.Equal(AppResources.Wikipedia_FinalTitle, Driver.Title);
        }

        [Fact]
        public void FillFormToSearchTrain()
        {
            //Arrange
            Driver.Navigate().GoToUrl(AppResources.Train_Url);
            var tomorrow = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            const string hour = "06:30";

            //Act
            Driver.FindElement(By.Id("przyciskZamknijNowyPopup")).Click();

            //await for popup to disappear
            WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
            wait.Until(drv => drv.FindElement(By.CssSelector("div.k-widget.k-window")).Displayed);

            Driver.FindElement(By.Name("ko_unique_1_input")).SendKeys(AppResources.Train_StartStation);

            Driver.FindElement(By.Name("ko_unique_2_input")).SendKeys(AppResources.Train_EndStation);

            var date = Driver.FindElement(By.Name("ko_unique_3"));
            date.Clear();
            date.SendKeys(tomorrow);

            var time = Driver.FindElement(By.Name("ko_unique_4"));
            time.Clear();
            time.SendKeys(hour);

            Driver.FindElement(By.ClassName("wp-parameters-arrow")).Click();

            Driver.FindElement(By.CssSelector("span.item")).Click();
            var js = (IJavaScriptExecutor)Driver;
            js.ExecuteScript("document.getElementById('kryteria-wyszukiwania').scrollIntoView();");

            var criteria = Driver.FindElements(By.CssSelector(
                "#kryteria-wyszukiwania > div > article.sBox.parameters.searcher > div.container.wp-parameters > div:nth-child(1) > div.row.paramsSimple.act > div.multiMenu.parametersMenu.active > div > ul > li"));
            foreach (var checkbox in criteria)
            {
                var trainType = checkbox.FindElement(By.CssSelector("label > span"));
                if (trainType.Text != "lokalny")
                {
                    checkbox.FindElement(By.CssSelector("label > input[type='checkbox']")).Click();
                }
            }

            Driver.FindElement(By.CssSelector("div.layer > div.btnRow > button")).Click();
            Driver.FindElement(By.CssSelector("div.carrier > div > div > span")).Click();

            var carriers = Driver.FindElements(By.CssSelector("div.carrier > div > div > div > ul > li"));
            foreach (var checkbox in carriers)
            {
                var trainCarrier = checkbox.FindElement(By.CssSelector("label > span"));
                if (trainCarrier.Text != @"""Przewozy Regionalne"" sp. z o.o.")
                {
                    checkbox.FindElement(By.CssSelector("label > input[type='checkbox']")).Click();
                }
            }

            Driver.FindElement(By.CssSelector(" div.carrier > div > div > div > div > button:nth-child(1)")).Click();

            WebDriverWait waiter = new WebDriverWait(Driver, TimeSpan.FromMilliseconds(1500));
            waiter.Until(drv => drv.FindElement(By.CssSelector("div.carrier > div > div > div")).GetCssValue("display") != "none");

            Driver.FindElement(By.Id("polaczeniaSzukaj")).Click();

            //Assert
            waiter.Until(drv =>
                drv.FindElement(By.CssSelector("#podsumowanie-kryteriow-wyszukiwania > div > div:nth-child(1)")).Text.Length > 0); //Wait until site is not loaded

            Assert.Equal(AppResources.Train_TitleToAssert, Driver
                .FindElement(By.CssSelector("#podsumowanie-kryteriow-wyszukiwania > div > div:nth-child(1)")).Text);
            Assert.Equal(tomorrow, Driver.FindElement(By.CssSelector("#podsumowanie-kryteriow-wyszukiwania > div > div:nth-child(3) > span:nth-child(3)")).Text);
            Assert.Equal(hour, Driver.FindElement(By.CssSelector("#podsumowanie-kryteriow-wyszukiwania > div > div:nth-child(3) > span:nth-child(4)")).Text);
        }

        [Fact]
        private void FindWithSelects()
        {
            //Arrange
            Driver.Navigate().GoToUrl(AppResources.Ebay_URL);
            var priceCheckbox = Driver.FindElement(By.Id("_mPrRngCbx"));

            //Act
            var productName = Driver.FindElement(By.Id("_nkw"));
            productName.SendKeys(AppResources.Ebay_ItemToSearch);

            SelectElement selectCategory = new SelectElement(Driver.FindElement(By.Id("e1-1")));
            selectCategory.SelectByText(AppResources.Ebay_Category);

            Driver.FindElement(By.Id("LH_TitleDesc")).Click();

            var minimumPrice = Driver.FindElement(By.CssSelector("#adv_search_from > fieldset:nth-child(3) > label > input"));
            minimumPrice.SendKeys(AppResources.Ebay_MinimumPrice);

            var maximumPrice = Driver.FindElement(By.CssSelector("#adv_search_from > fieldset:nth-child(3) > input.price"));
            maximumPrice.SendKeys(AppResources.Ebay_MaximumPrice);


            //dismiss banner
            if (CheckIfBannerIsDisplayed())
            {
                Driver.FindElement(By.Id("gdpr-banner-accept")).Click();
            }

            Driver.FindElement(By.Id("LH_BIN")).Click();
            Driver.FindElement(By.Id("LH_ItemConditionNew")).Click();
            Driver.FindElement(By.Id("LH_FS")).Click();
            Driver.FindElement(By.Id("LH_SubLocation")).Click();

            SelectElement radius = new SelectElement(Driver.FindElement(By.Id("_sadis")));
            radius.SelectByValue(AppResources.Ebay_ProductDistanceRadius);
            Driver.FindElement(By.Id("_fpos")).SendKeys("85-796");


            var country = Driver.FindElement(By.Id("_sargnSelect"));
            SelectElement countrySelect = new SelectElement(country);
            countrySelect.SelectByText(AppResources.Ebay_Country);

            SelectElement selectCriterion = new SelectElement(Driver.FindElement(By.Id("LH_SORT_BY")));
            selectCriterion.SelectByIndex(4);

            SelectElement itemsCount = new SelectElement(Driver.FindElement(By.Id("LH_IPP")));
            itemsCount.SelectByIndex(2);


            //Assert
            Assert.True(Driver.FindElement(By.Id("LH_ITEMS_NEAR_ME")).Selected);
            Assert.True(country.Enabled);
            Assert.True(priceCheckbox.Selected);

            //Assert after navigation to results tab
            Driver.FindElement(By.Id("searchBtnLowerLnk")).Click();
            Assert.Equal(AppResources.Ebay_TitleToAssert, Driver.Title);
            Assert.Equal(AppResources.Ebay_MinimumPrice, Driver.FindElement(By.Id("e1-10")).GetAttribute("Value"));
            Assert.Equal(AppResources.Ebay_MaximumPrice, Driver.FindElement(By.Id("e1-11")).GetAttribute("Value"));
            Assert.Equal(AppResources.Common_TrueLowerCase, Driver.FindElement(By.Id("e1-25")).GetAttribute("checked"));
            Assert.Equal(AppResources.Common_TrueLowerCase, Driver.FindElement(By.Id("e1-38")).GetAttribute("Checked"));
        }
        #endregion

        #region PrivateMethods
        private bool CheckIfElementHasSpecifiedText(string cssSelector, string data)
        {
            try
            {
                return Driver.FindElement(By.CssSelector(cssSelector)).Text.Equals(data);
            }
            catch (NoSuchElementException ex)
            {
                return false;
            }
        }

        private bool CheckIfBannerIsDisplayed()
        {
            try
            {
                return Driver.FindElement(By.XPath("//*[@id=\"gdpr-banner\"]")).Displayed;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        public void Dispose()
        {
            try
            {
                Driver.Quit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{AppResources.Common_DisposeBrowserError} {ex}");
            }
        }
    }
}