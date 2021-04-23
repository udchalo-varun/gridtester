using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace GridTester
{
    internal class Grid
    {
        private decimal low;
        private decimal high;
        private decimal gridGap;
        private int gridQty;
        private decimal startPrice;
        private readonly List<StockDayInfo> tradingFile;
        private readonly int gridLotSize;
        private readonly decimal startingCash;
        private decimal availableCash;
        private readonly decimal startingStockQuantityHold;
        private List<GridRow> gridRows;
        private List<string> logs = new List<string>();
        private List<string> logsCsv = new List<string>();
        private List<Trade> trades = new List<Trade>();
        private int numTrades = 0;
        public decimal GridProfit { get; set; }
        public decimal AfterCloseSquareOff { get; private set; }

        public decimal MaxCashNeeded { get; private set; }
        public decimal CashNeeded { get; private set; }
        public DateTime CloseDate { get; private set; }
        public DateTime OpenDate { get; private set; }
        public decimal CloseValue { get; private set; }
        public decimal HoDLCloseValue { get; private set; }
        public decimal OpenValue { get; private set; }

        public Grid(decimal low, decimal high, decimal gridGap, int gridQty, int gridLotSize, decimal startPrice, decimal startingCash, decimal startingStockHold, List<StockDayInfo> tradingFile)
        {
            this.low = low;
            this.high = high;
            this.gridGap = gridGap;
            this.gridQty = gridQty;
            this.startPrice = startPrice;
            this.tradingFile = tradingFile;
            this.gridLotSize = gridLotSize;
            this.startingCash = startingCash;
            this.availableCash = startingCash;
            this.startingStockQuantityHold = startingStockHold;
        }

        internal void Initialize()
        {
            gridRows = new List<GridRow>();

            for (int i = gridQty - 1; i >= 0; i--)
            {
                gridRows.Add(new GridRow(low + (i * gridGap), gridGap));
            }
            this.OpenDate = tradingFile[0].Date;
            
        }

        internal void Publish()
        {
            foreach(var log in logs)
            {
                Console.WriteLine(log);
                Thread.Sleep(100);
            }
            this.WriteCsv();
            Console.ReadLine();
        }

        internal void Run()
        {
            this.OpenValue = tradingFile[0].OpenPrice * startingStockQuantityHold + startingCash;
            logsCsv.Add(this.CsvOpen());
            int i = 0;
            for (; i < tradingFile.Count; i++)
            {
                if (tradingFile[i].LowPrice < low || tradingFile[i].HighPrice > high)
                {
                    logs.Add(string.Format("Bot Low/High hit on " + tradingFile[i].Display()));
                    logsCsv.Add("closed");
                    break;
                }

                //Check If trade can open
                var gridRow = gridRows.Find(x => x.BuyPrice <= tradingFile[i].LowPrice);

                if (!gridRow.IsBuy)
                {
                    gridRow.IsBuy = true;
                    var trade = new Trade(gridRow.BuyPrice, gridLotSize, gridRow.SellPrice, tradingFile[i].Date, tradingFile[i].Equity, gridRow);
                    logs.Add(trade.DisplayOpen());
                    logsCsv.Add(trade.CsvOpen());
                    trades.Add(trade);
                    var cash = (trade.BuyPrice * trade.Quantity);
                    CashNeeded += cash;
                    availableCash -= cash;
                    if (CashNeeded > MaxCashNeeded)
                        MaxCashNeeded = CashNeeded;
                }

                //Check Trade can close
                var tradesToClose = trades.FindAll(x => x.SellPrice <= tradingFile[i].HighPrice);

                foreach(var trade in tradesToClose)
                {
                    trade.DateClose = tradingFile[i].Date;
                    trade.GridRow.IsBuy = false;
                    logs.Add(trade.DisplayClose());
                    logsCsv.Add(trade.CsvClose());

                    GridProfit += (trade.Quantity * (trade.SellPrice - trade.BuyPrice));
                    trades.Remove(trade);
                    var cash = (trade.SellPrice * trade.Quantity);
                    CashNeeded -= cash;
                    availableCash += cash;
                    numTrades++;
                }

            }

            var lastDay = tradingFile[i >= tradingFile.Count ? tradingFile.Count - 1 : i];


            if (trades.Count > 0)
            {
                //Trades still open/ Write square off value
                foreach (var trade in trades)
                {

                    trade.DateClose = lastDay.Date;
                    trade.GridRow.IsBuy = false;
                    logs.Add(trade.DisplayClose());
                    logsCsv.Add(trade.CsvClose());

                    AfterCloseSquareOff += (trade.Quantity * (lastDay.ClosePrice - trade.BuyPrice));
                    //trades.Remove(trade);
                }
            }

            this.CloseDate = lastDay.Date;
            this.CloseValue = lastDay.ClosePrice * startingStockQuantityHold + availableCash + AfterCloseSquareOff;
            this.HoDLCloseValue = lastDay.ClosePrice * startingStockQuantityHold + startingCash;
            //Display Grid Performance
            logs.Add(this.Display());
            logsCsv.Add(this.CsvClose());


        }

        private string CsvClose()
        {
            return string.Join(",", CloseDate, "Instrument", "Close", OpenValue, CloseValue, (int)CloseDate.Subtract(OpenDate).TotalDays,
                HoDLCloseValue, CloseValue - OpenValue, CloseValue - HoDLCloseValue);

        }

        private string CsvOpen()
        {
            return string.Join(",", "Date", "Instrument", "Action", "BuyPrice", "SellPrice", "DaysHeld", "Profit/Loss", "Brokerage", "Net P/L");
        }

        private string Display()
        {
            return string.Format(
                "Grid Stats: Open Date: {0}, Close Date: {1}, MaxCash: {2}, GridProfit: {3}, OpenValue: {4}, CloseValue: {5}, HoDLCloseValue: {6}, " +
                "P&L vs Hold: {7}, Num Trades: {8}",
                OpenDate, CloseDate, MaxCashNeeded, GridProfit, OpenValue, CloseValue, HoDLCloseValue, CloseValue - HoDLCloseValue, numTrades);
        }

        private void WriteCsv()
        {
            File.WriteAllLines("GridTester-Output" + Environment.TickCount + ".csv", logsCsv);
        }
    }
}