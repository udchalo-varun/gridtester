using System;

namespace GridTester
{
    internal class Trade
    {
        

        public Trade(decimal buyPrice, int qty, decimal sellPrice, DateTime date, string equity, GridRow gridRow)
        {
            this.BuyPrice = buyPrice;
            this.Quantity = qty;
            this.SellPrice = sellPrice;
            this.DateOpen = date;
            this.GridRow = gridRow;
            this.Brokerage = 8;
            this.Equity = equity;
        }

        public decimal BuyPrice { get; }
        public int Quantity { get; }
        public decimal SellPrice { get; }
        public DateTime DateOpen { get; }
        public GridRow GridRow { get; }
        public int Brokerage { get; }
        public string Equity { get; }
        public DateTime DateClose { get; set; }

        internal string DisplayOpen()
        {
            return string.Format("Trade Open: Date: {0}, BuyPrice: {1}", DateOpen, BuyPrice);
        }

        internal string DisplayClose()
        {
            return string.Format("Trade Close: Date: {0}, SellPrice: {1}", DateClose, SellPrice);

        }

        internal string CsvOpen()
        {
            return string.Join(",", DateOpen.ToShortDateString(), Equity, "Buy", BuyPrice, "", "", "", Brokerage, "");
        }

        internal string CsvClose()
        {
            return string.Join(",", DateClose.ToShortDateString(), Equity, "Sell", BuyPrice, SellPrice, (int)DateClose.Subtract(DateOpen).TotalDays,
                SellPrice - BuyPrice, Brokerage, SellPrice - BuyPrice - Brokerage);
        }
    }
}