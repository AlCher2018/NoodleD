using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppModel
{
    // класс для хранения выбранных пунктов меню (блюд)
    public class OrderItem
    {
        public int OrderNumberForPrint { get; set; }
        public DateTime OrderDate { get; set; }

        private List<DishItem> _disheItems;

        public List<DishItem> Dishes { get { return _disheItems; } }

        public OrderItem()
        {
            _disheItems = new List<DishItem>();
        }

        // добавить КОПИЮ DishItem
        public void AddDish(DishItem dish)
        {
            if (dish != null)
            {
                _disheItems.Add(dish);
            }
        }
        public void RemoveDish(DishItem dish)
        {
            if (dish != null) _disheItems.Remove(dish);
        }

        private bool isExist(DishItem checkingItem)
        {
            return _disheItems.Any(d => d.RowGUID == checkingItem.RowGUID);
        }

        public decimal GetOrderValue()
        {
            decimal retVal = 0;
            if (this.Dishes != null)
                foreach (DishItem item in Dishes) retVal += item.GetValueInOrder();
            
            return retVal;
        }

    } // class CurrentOrder

}
