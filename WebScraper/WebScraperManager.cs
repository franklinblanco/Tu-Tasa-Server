using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace WebScraper
{
    class WebScraperManager
    { //Class responsible for doing all web-scraping related stuff. 
        #region Singleton Pattern
        private static WebScraperManager instance = new WebScraperManager();
        private WebScraperManager() { }

        public static WebScraperManager Instance
        {
            get { return instance; }
        }
        #endregion

        public List<CurrencyRate> DailyScrape() //This method just scrapes all the banks in the array for a the rates, and returns them. 
            //Called by the CacheManager
        {

            CurrencyRate[] defaultRates = new CurrencyRate[11]; //This is where all of the neccesary information for scraping each bank is stored, since
            //The server has no database yet, these values are hardcoded.
            #region currency rate hardcode
            defaultRates[0] = new CurrencyRate { bankname = "BHDLeon", bankurl = "https://www.bhdleon.com.do/wps/portal/BHD/Inicio", currency = "USD", needclick = true, xpathstocurrency = new string[] { "//*[@id='TasasDeCambio']/table/tbody/tr[2]/td[2]", "//*[@id='TasasDeCambio']/table/tbody/tr[2]/td[3]" }, xpathtoclick = "//*[@id='footer']/section[2]/div/ul/li[5]/a" };
            defaultRates[1] = new CurrencyRate { bankname = "Popular", bankurl = "https://www.popularenlinea.com/", currency = "USD", needclick = false, xpathstocurrency = new string[] { "//*[@id='compra_peso_dolar_desktop']", "//*[@id='venta_peso_dolar_desktop']" }, xpathtoclick = "", currencyisinputfield = true };
            defaultRates[2] = new CurrencyRate { bankname = "Banreservas", bankurl = "https://www.banreservas.com/", currency = "USD", needclick = false, xpathstocurrency = new string[] { "/html/body/header/div[1]/div/div[1]/div[3]/div/ul/li[1]/span", "/html/body/header/div[1]/div/div[1]/div[3]/div/ul/li[2]/span" }, xpathtoclick = "" };
            defaultRates[3] = new CurrencyRate { bankname = "BancoCaribe", bankurl = "https://www.bancocaribe.com.do/", currency = "USD", needclick = true, xpathstocurrency = new string[] { "//*[@id='us_buy_res']", "//*[@id='us_sell_res']" }, xpathtoclick = "//*[@id='exchange-rates-button']" };
            defaultRates[4] = new CurrencyRate { bankname = "ScotiaBank", bankurl = "https://do.scotiabank.com/banca-personal/tarifas/tasas-de-cambio.html", currency = "USD", needclick = false, xpathstocurrency = new string[] { "//*[@id='main']/div/div/div/div/div[4]/table/tbody/tr[2]/td[3]", "//*[@id='main']/div/div/div/div/div[4]/table/tbody/tr[2]/td[4]" } };
            defaultRates[5] = new CurrencyRate { bankname = "BancoAdemi", bankurl = "https://bancoademi.com.do/", currency = "USD", needclick = true, xpathstocurrency = new string[] { "/html/body/div[1]/div/div/div[2]/fieldset/div[1]/div[4]/div/input", "/html/body/div[1]/div/div/div[2]/fieldset/div[2]/div[4]/div/input" }, xpathtoclick = "//*[@id='menuTop']/div/div/a[5]", currencyisplaceholder = true, currencyisinputfield = true };
            defaultRates[6] = new CurrencyRate { bankname = "BancoPromerica", bankurl = "https://www.promerica.com.do/", currency = "USD", needclick = false, xpathstocurrency = new string[] { "//*[@id='tipoCambioHome']/div[2]/p/span[1]", "//*[@id='tipoCambioHome']/div[2]/p/span[3]" } };
            defaultRates[7] = new CurrencyRate { bankname = "BancoVimenca", bankurl = "https://www.bancovimenca.com/", currency = "USD", needclick = false, xpathstocurrency = new string[] { "//*[@id='exangeRates']/li[1]/div/div/div[1]/article", "//*[@id='exangeRates']/li[1]/div/div/div[2]/article" } };
            defaultRates[8] = new CurrencyRate { bankname = "BancoBDI", bankurl = "https://www.bdi.com.do/", currency = "USD", needclick = false, xpathstocurrency = new string[] { "//*[@id='dnn_ctr421_ModuleContent']/div/div/div/div[2]/div[1]/ul/li[3]", "//*[@id='dnn_ctr421_ModuleContent']/div/div/div/div[2]/div[1]/ul/li[4]" } };
            defaultRates[9] = new CurrencyRate { bankname = "Bancamerica", bankurl = "https://bancamerica.com.do/", currency = "USD", needclick = false, xpathstocurrency = new string[] { "/html/body/section[2]/div[1]/div/div/div/div[1]/ul/li[1]/strong[1]", "/html/body/section[2]/div[1]/div/div/div/div[1]/ul/li[1]/strong[2]" } };
            defaultRates[10] = new CurrencyRate { bankname = "BellBank", bankurl = "https://www.bellbank.com/", currency = "USD", needclick = false, xpathstocurrency = new string[] { "/html/body/div[2]/div[3]/span/span", "/html/body/div[2]/div[3]/span/span" }, shouldformat = true, separator = '/'};
            #endregion
            List<CurrencyRate> FinishedRates = new List<CurrencyRate>();
            foreach (CurrencyRate rate in defaultRates)
            {
                CurrencyRate scrapedRate = ScrapeCurrencyRate(rate);
                if (scrapedRate.buyrate == "ERROR" || scrapedRate.sellrate == "ERROR") { ScrapeAgainLater(rate); }
                FinishedRates.Add(rate);
            }
            return FinishedRates;
        }
        public CurrencyRate ScrapeCurrencyRate(CurrencyRate currencyRate) //This is the method that does the hard work. 
            //It goes to each website and simulates user interactions to get values.
        {
            try
            {
                var chromeOptions = new ChromeOptions();
                chromeOptions.AddArguments("--headless", "--no-sandbox", "--disable-dev-shm-usage", "--window-size=1920,1080", "--start-maximized");
                using (var browserdriver = new ChromeDriver(chromeOptions))
                {
                    browserdriver.Navigate().GoToUrl(currencyRate.bankurl); //go to the url

                    if (currencyRate.needclick)
                    {
                        //wait before clicking (Explicitly)
                        var wait = new WebDriverWait(browserdriver, TimeSpan.FromSeconds(3));
                        wait.Until(drv => drv.FindElement(By.XPath(currencyRate.xpathtoclick))).Click();
                        //var buttontoclick = browserdriver.FindElement(By.XPath(currencyRate.xpathtoclick));
                        //buttontoclick.Click();
                    }

                    string[] currencyrates = new string[2];

                    if (currencyRate.currencyisinputfield) //Self explanatory, but this basically checks if it is an input field so that it gets the "Value" instead of 
                        //the "Text" which would give an exception.
                    {
                        if (currencyRate.currencyisplaceholder)
                        {
                            currencyRate.buyrate = browserdriver.FindElement(By.XPath(currencyRate.xpathstocurrency[0])).GetAttribute("placeholder");
                            currencyRate.sellrate = browserdriver.FindElement(By.XPath(currencyRate.xpathstocurrency[1])).GetAttribute("placeholder");

                        }
                        else //Differentiate between input placeholder & value
                        {
                            currencyRate.buyrate = browserdriver.FindElement(By.XPath(currencyRate.xpathstocurrency[0])).GetAttribute("value");
                            currencyRate.sellrate = browserdriver.FindElement(By.XPath(currencyRate.xpathstocurrency[1])).GetAttribute("value");
                        }
                    }
                    else
                    {
                        var buyrateinpage = browserdriver.FindElement(By.XPath(currencyRate.xpathstocurrency[0]));
                        var sellrateinpage = browserdriver.FindElement(By.XPath(currencyRate.xpathstocurrency[1]));

                        //Format
                        if (currencyRate.shouldformat)
                        {
                            string[] separatedText = buyrateinpage.Text.Split('/');
                            currencyRate.buyrate = TrimString(separatedText[0]);
                            currencyRate.sellrate = TrimString(separatedText[1]);
                        }
                        else
                        {
                            if (buyrateinpage.Text != "" || sellrateinpage.Text != "")
                            {
                                currencyRate.buyrate = TrimString(buyrateinpage.Text);
                                currencyRate.sellrate = TrimString(sellrateinpage.Text);
                                //Trim all the strings of their whitespace so that rates are kept very nicely.
                            }
                            else
                            {
                                currencyRate.buyrate = TrimString(buyrateinpage.GetAttribute("textContent"));
                                currencyRate.sellrate = TrimString(sellrateinpage.GetAttribute("textContent"));
                            }
                        }
                    }
                    browserdriver.Close();
                    browserdriver.Dispose();
                    browserdriver.Quit();
                    Console.WriteLine("Closing BrowserDriver");
                }

                Console.WriteLine(currencyRate.bankname + " Buys at: " + currencyRate.buyrate + " | Sells at: " + currencyRate.sellrate);
                return currencyRate;
            } catch (Exception e)
            {
                Console.WriteLine("Exception occurred in a scraping. Sending Empty value, Trying again Later.");
                Console.WriteLine(e.ToString());
                return currencyRate;
            }
            
        }
        private async void ScrapeAgainLater(CurrencyRate Rate)
        {
            CurrencyRate rate = Rate;
            while (rate.buyrate == "ERROR" || rate.sellrate == "ERROR")
            {
                Console.WriteLine("Unsuccesful scrape. Trying again in 15 mins...");
                await Task.Delay(TimeSpan.FromMinutes(15)); //every 15 mins, try again
                rate = ScrapeCurrencyRate(rate);
            }
            CacheManager.Instance.BankRates[rate.bankname] = rate;
            Console.WriteLine("Cache Updated correctly with the currency rate: " + rate.bankname);
        }

        public string TrimString(string stringtotrim) //format method to remove all common letters, characters, and signs used in currencies, to get the numbers.
        {
            return stringtotrim.Trim(' ', 'D', 'O', 'P', 'R', 'D', '$', 'C', 'm', 'a', 'V', 'e', 'n', 't', 'o', 'p', 'r', ':', 'U', 'S', 'D');
        }
        
    }
}
