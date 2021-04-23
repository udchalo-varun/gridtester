using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTester
{
    public class StockInfoLoader
    {
        public List<StockDayInfo> ParseFile(string fileName)
        {
            var stockList = new List<StockDayInfo>();
            using (var csvReader = new CsvFileReader(fileName))
            {
                while (!csvReader.EndOfData())
                {
                    //Read Line
                    var line = csvReader.ReadLine();

                    if (line?.Count > 0 && line[0] != "Symbol")
                    {
                        stockList.Add(new StockDayInfo
                        {
                            Equity = line[0],
                            Date = DateTime.Parse(line[2]),
                            OpenPrice = decimal.Parse(line[4]),
                            HighPrice = decimal.Parse(line[5]),
                            LowPrice = decimal.Parse(line[6]),
                            ClosePrice = decimal.Parse(line[8]),
                            Volume = int.Parse(line[12])
                        });
                    }
                }
            }
            return stockList;
        }
    }
}
