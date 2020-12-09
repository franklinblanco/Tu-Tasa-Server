using System;
using System.Collections.Generic;
using System.Text;

namespace WebScraper
{
    class CacheManager //This class is to temporarily store data, as there is no database.
    {
        #region Singleton Pattern
        private static CacheManager instance = new CacheManager();
        private CacheManager() { }

        public static CacheManager Instance
        {
            get { return instance; }
        }
        #endregion

        public Dictionary<string, CurrencyRate> BankRates = new Dictionary<string, CurrencyRate>(); //BankName & currencyrate array to store all the currencyholds

        public void Initialize()
        {
            UpdateCache();
        }
        public void UpdateCache()
        {
            List<CurrencyRate> currencyRates = WebScraperManager.Instance.DailyScrape();
            foreach (CurrencyRate rate in currencyRates)
            {
                BankRates.Add(rate.bankname, rate);
            }
        }
    }
}
