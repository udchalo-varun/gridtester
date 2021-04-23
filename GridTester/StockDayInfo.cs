using System;

namespace GridTester
{
    public class StockDayInfo
    {
        public DateTime Date { get; internal set; }
        public decimal OpenPrice { get; internal set; }
        public decimal HighPrice { get; internal set; }
        public decimal LowPrice { get; internal set; }
        public decimal ClosePrice { get; internal set; }
        public int Volume { get; internal set; }
        public string Equity { get; internal set; }

        internal string Display()
        {
            return string.Format("Date: {0}, Open: {1}, Close: {2}, High: {3}, Low: {4}", Date, OpenPrice, ClosePrice, HighPrice, LowPrice);
        }
    }
}