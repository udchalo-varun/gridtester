using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTester
{
    public class GridTester
    {
        private string _fileName;
        private List<StockDayInfo> _stockList = new List<StockDayInfo>();
        private decimal _low;
        private decimal _high;
        private decimal _gridGap;
        private int _gridQty;
        private decimal _startPrice;
        private int _gridLotSize;
        private decimal _startingCash;
        private decimal _startingStocksHeldPercentForBot;
        private bool _reinvestGridProfit;

        public GridTester(string fileName)
        {
            this._fileName = fileName;
            var loader = new StockInfoLoader();
            _stockList = loader.ParseFile(fileName);

            //Read Config
            var variable = decimal.Parse(ConfigurationManager.AppSettings["Variable"]);
            var trigger = decimal.Parse(ConfigurationManager.AppSettings["Trigger"]);
            var triggerDate = string.IsNullOrEmpty(ConfigurationManager.AppSettings["TriggerDate"]) ? DateTime.MinValue : DateTime.Parse(ConfigurationManager.AppSettings["TriggerDate"]);
            var gridLot = int.Parse(ConfigurationManager.AppSettings["GridLot"]);
            var startingProfitPerGrid = decimal.Parse(ConfigurationManager.AppSettings["InitialGridProfit"]);
            _startingCash = decimal.Parse(ConfigurationManager.AppSettings["StartingCash"]);
            _startingStocksHeldPercentForBot = decimal.Parse(ConfigurationManager.AppSettings["StartingStocksPercent"]);
            _reinvestGridProfit = bool.Parse(ConfigurationManager.AppSettings["ReInvestGridProfit"]);

            if(triggerDate > DateTime.MinValue)
            {
                trigger = _stockList.Find(x => x.Date >= triggerDate).OpenPrice;
                _stockList.RemoveAll(x => x.Date < triggerDate);
            }


            _low = trigger * (1 - variable);
            _high = trigger * (1 + variable);
            _gridGap = decimal.Round(trigger * startingProfitPerGrid, 2);
            _gridQty = (int)((_high - _low) / _gridGap);
            _startPrice = trigger;
            _gridLotSize = gridLot;
        }

        internal void Run()
        {
            if (_stockList.Count == 0)
                return;

            var grid = new Grid(_low, _high, _gridGap, _gridQty, _gridLotSize, _startPrice, _startingCash, _startingStocksHeldPercentForBot, _stockList, _reinvestGridProfit);

            grid.Initialize();
            grid.Run();
            grid.Publish();
        }
    }
}
