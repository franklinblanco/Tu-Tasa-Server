using System;
using System.Collections.Generic;
using System.Text;

namespace WebScraper
{
    class CurrencyRate
    {
        public string bankname;
        public string currency;

        public string buyrate = "ERROR";
        public string sellrate = "ERROR";

        public string bankurl;
        public string[] xpathstocurrency; //must be of size 2, buyrate goes first, sellrate goes second.
        public bool needclick = false;
        public string xpathtoclick;

        public bool currencyisinputfield = false;
        public bool currencyisplaceholder = false;
        public bool shouldformat = false;
        public char separator;

    }
}
