namespace GridTester
{
    internal class GridRow
    {
        private int numBuys;
        private int numSells;
        public GridRow(decimal buyPrice, decimal gridGap)
        {
            this.BuyPrice = buyPrice;
            this.gridGap = gridGap;
            this.SellPrice = buyPrice + gridGap * 2;
        }

        public decimal BuyPrice { get; }

        private decimal gridGap;

        public decimal SellPrice { get; }
        public bool IsBuy { get; internal set; }
    }
}