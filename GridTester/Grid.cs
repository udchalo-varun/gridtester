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
        private readonly decimal startingStockHoldPercent;
        private List<GridRow> gridRows;
        //private List<string> logs = new List<string>();
        private List<string> logsCsv = new List<string>();
        private List<Trade> trades = new List<Trade>();
        private int numTrades = 0;
        public decimal GridProfitCurrent { get; set; }
        public decimal GridProfitTotal { get; set; }

        public int GridProfitShares { get; set; }
        public decimal AfterCloseSquareOffSharesHeld { get; private set; }

        public decimal MaxCashNeeded { get; private set; }
        public decimal CashNeeded { get; private set; }
        public DateTime CloseDate { get; private set; }
        public DateTime OpenDate { get; private set; }
        public decimal CloseValue { get; private set; }
        public decimal HoDLCloseValue { get; private set; }
        public decimal OpenValue { get; private set; }
        public int OpenHoldHODLQuantity { get; private set; }
        public int OpenStocksQuantityForBot { get; private set; }
        public bool IsReinvestGridProfit { get; }

        public Grid(decimal low, decimal high, decimal gridGap, int gridQty, int gridLotSize, decimal startPrice, decimal startingCash, decimal startingStockHoldPercent, List<StockDayInfo> tradingFile, bool isReinvestGridProfit)
        {
            this.low = low;
            this.high = high;
            this.gridGap = gridGap;
            this.gridQty = gridQty;
            this.startPrice = startPrice;
            this.tradingFile = tradingFile;
            IsReinvestGridProfit = isReinvestGridProfit;
            this.gridLotSize = gridLotSize;
            this.startingCash = startingCash;
            this.availableCash = startingCash;
            this.startingStockHoldPercent = startingStockHoldPercent;
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
            //foreach (var log in logs)
            foreach (var log in logsCsv)
            {
                Console.WriteLine(log);
                Thread.Sleep(100);
            }
            this.WriteCsv();
            Console.ReadLine();
        }

        internal void Run()
        {
            int i = 0;
            var currentDay = tradingFile[i];

            this.OpenValue = startingCash;
            this.OpenHoldHODLQuantity = (int)(startingCash / currentDay.OpenPrice);
            //reserve 4 lots worth of money for bot cash
            var qtyToReserveForBotCash = gridLotSize * 4;
            this.OpenStocksQuantityForBot = (int)(OpenHoldHODLQuantity - qtyToReserveForBotCash);
            this.availableCash = startingCash - OpenStocksQuantityForBot * currentDay.OpenPrice;
            
            logsCsv.Add(this.CsvOpen());

            for (; i < tradingFile.Count; i++)
            {
                currentDay = tradingFile[i];
                if (currentDay.LowPrice < low || currentDay.HighPrice > high)
                {
                    //logs.Add(string.Format("Bot Low/High hit on " + currentDay.Display()));
                    logsCsv.Add("closed");
                    break;
                }

                //Check If trade can open
                var gridRow = gridRows.Find(x => x.BuyPrice <= currentDay.LowPrice);

                if (!gridRow.IsBuy)
                {
                    gridRow.IsBuy = true;
                    var trade = new Trade(gridRow.BuyPrice, gridLotSize, gridRow.SellPrice, currentDay.Date, currentDay.Equity, gridRow);
                    //logs.Add(trade.DisplayOpen());
                    logsCsv.Add(trade.CsvOpen());
                    var cash = (trade.BuyPrice * trade.Quantity);
                    CashNeeded += cash;
                    if ((availableCash - cash) < 0)
                    {
                        var text = string.Format("Bot cash shortfall. Required: {0}, Available cash: {1} ", cash, availableCash);
                        //logs.Add(text); //+ currentDay.Display()));
                        logsCsv.Add(text);
                        break;
                    }

                    availableCash -= cash;

                    if (CashNeeded > MaxCashNeeded)
                        MaxCashNeeded = CashNeeded;
                    trades.Add(trade);

                }

                //Check Trade can close
                var tradesToClose = trades.FindAll(x => x.SellPrice <= currentDay.HighPrice);

                foreach (var trade in tradesToClose)
                {
                    trade.DateClose = currentDay.Date;
                    trade.GridRow.IsBuy = false;
                    //logs.Add(trade.DisplayClose());
                    logsCsv.Add(trade.CsvClose());

                    GridProfitCurrent += (trade.Quantity * (trade.SellPrice - trade.BuyPrice));
                    GridProfitTotal += (trade.Quantity * (trade.SellPrice - trade.BuyPrice));
                    trades.Remove(trade);
                    var cash = (trade.SellPrice * trade.Quantity);
                    CashNeeded -= cash;
                    availableCash += cash;
                    numTrades++;
                    if (IsReinvestGridProfit)
                    {
                        //Try to maintain at least 3 lots of next grid size available
                        //Get next higher grid to purchase
                        var minBalance = trade.SellPrice * 4m * gridLotSize;
                        if (availableCash > minBalance)
                        {
                            //Buy extra shares at current price
                            var sharesToBuy = (int)Math.Floor((availableCash - minBalance) / currentDay.ClosePrice);
                            if (sharesToBuy > 0)
                            {
                                //Update currentGridProfit
                                availableCash -= sharesToBuy * currentDay.ClosePrice;

                                //Update grid shares bought extra
                                GridProfitShares += sharesToBuy;

                            }
                        }
                    }
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
                    //logs.Add(trade.DisplayClose());
                    logsCsv.Add(trade.CsvClose());

                    AfterCloseSquareOffSharesHeld += trade.Quantity;
                    //trades.Remove(trade);
                }
            }

            this.CloseDate = lastDay.Date;
            this.CloseValue = lastDay.ClosePrice * (OpenStocksQuantityForBot + GridProfitShares + AfterCloseSquareOffSharesHeld) + availableCash;
            this.HoDLCloseValue = lastDay.ClosePrice * OpenHoldHODLQuantity;
            //Display Grid Performance
            //logs.Add(this.Display());
            logsCsv.Add(this.CsvClose());


        }

        private string CsvClose()
        {
            var header = string.Join(",", "CloseDate", "Equity", "Action", "OpenValue", 
                "CloseValue", "P&L", "P&L%", 
                "DaysRun",
                "HoDLCloseValue", "HoDLP&L", "HoDLP&L%", 
                "P&L vs HODL", "P&L vs HODL%", 
                "numTrades", "GridProfitTotal", "GridProfitShares", "AvailableCash");
            var item = string.Join(",", CloseDate, tradingFile[0].Equity, "Close", OpenValue, CloseValue,
                CloseValue - OpenValue, decimal.Round(100 * (CloseValue - OpenValue) / OpenValue, 2),
                (int)CloseDate.Subtract(OpenDate).TotalDays,
                HoDLCloseValue, HoDLCloseValue - OpenValue, decimal.Round(100 * (HoDLCloseValue - OpenValue) / OpenValue, 2),
                CloseValue - HoDLCloseValue, decimal.Round(100 * (CloseValue - HoDLCloseValue) / HoDLCloseValue, 2),
                numTrades, GridProfitTotal, GridProfitShares, availableCash);
            return string.Join(Environment.NewLine, header, item);
        }

        private string CsvOpen()
        {
            var header = string.Join(",", "OpenDate", "Equity", "Action", "OpenValue", "OpenHODLQty", "BotOpenHoldQty",
                "BotCash");
            var item = string.Join(",", OpenDate, tradingFile[0].Equity, "Open", OpenValue, OpenHoldHODLQuantity, OpenStocksQuantityForBot,
                availableCash);
            var tradeHeader = string.Join(",", "Date", "Instrument", "Action", "BuyPrice", "SellPrice", "DaysHeld", "Profit/Loss", "Brokerage", "Net P/L");

            return string.Join(Environment.NewLine, header, item, tradeHeader);

        }

        private string Display()
        {
            //return string.Format(
            //    "Grid Stats: Open Date: {0}, Close Date: {1}, MaxCash: {2}, GridProfit: {3}, OpenValue: {4}, CloseValue: {5}, HoDLCloseValue: {6}, " +
            //    "P&L vs Hold: {7}, Num Trades: {8}",
            //    OpenDate, CloseDate, MaxCashNeeded, GridProfitCurrent, OpenValue, CloseValue, HoDLCloseValue, CloseValue - HoDLCloseValue, numTrades);
            return CsvClose();
        }

        private void WriteCsv()
        {
            File.WriteAllLines("GridTester-Output" + Environment.TickCount + ".csv", logsCsv);
        }
    }
}