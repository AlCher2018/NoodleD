using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppModel
{
    // класс для хранения выбранных пунктов меню (блюд)
    public class CurrentOrder
    {
        private List<CurrentDish> _dishesList;
        public CurrentOrder()
        {
            _dishesList = new List<CurrentDish>();

        }

        public void AddDish(DishItem dish)
        {
            if ((dish != null) && (isExist(dish) == false))
            {
                CurrentDish curDish = new CurrentDish();
                curDish.DishItem = dish;
                _dishesList.Add(curDish);
            }
        }
        public void RemoveDish(DishItem dish)
        {
            CurrentDish curDish = getExist(dish);
            if ((dish != null) && (curDish != null)) _dishesList.Remove(curDish);
        }

        public List<CurrentDish> GetDishes()
        {
            return _dishesList;
        }

        private CurrentDish getExist(DishItem checkingItem)
        {
            return _dishesList.FirstOrDefault(d => d.DishItem.Id == checkingItem.Id);
        }
        private bool isExist(DishItem checkingItem)
        {
            return _dishesList.Any(d => d.DishItem.Id == checkingItem.Id);
        }

    } // class CurrentOrder

    public class CurrentDish
    {
        public DishItem DishItem { get; set; }
        public DishAdding Garnish { get; set; }
        public List<DishAdding> Ingredients { get; set; }
    }

}
