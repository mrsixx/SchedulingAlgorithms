namespace Scheduling.Solver.Utils
{
    internal class RouletteWheelSelection<T>
    {
        public IList<RouletteWheelItem<T>> Items { get; } = [];

        public void AddItem(T item, double chance)
        {
            Items.Add(new RouletteWheelItem<T>(item, chance));
        }

        public T SelectItem()
        {
            var index = DrawIndex();
            if (index < 0)
                return default;
            return Items[index].Item;
        }

        int DrawIndex()
        {
            int n = Items.Count;

            double sum = Items.Sum(i => i.Chance);
            
            if (sum <= 0) // vals[] can't be all 0.0s
                throw new ArgumentOutOfRangeException("Some chance must be greather than 0.");

            double accum = 0.0;
            double p = Random.Shared.NextDouble();

            for (int i = 0; i < n; ++i)
            {
                accum += (Items[i].Chance / sum);
                if (p < accum)
                    return i;
            }
            return -1;  // not found
        }
    }

    internal class RouletteWheelItem<T>(T item, double chance)
    {
        public double Chance { get; set; } = chance;
        public T Item { get; set; } = item;
    }
}
