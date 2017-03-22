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
        public string DeviceID { get; set; }

        // номер заказа для печати чека
        private int _orderNumberForPrint;
        public int RangeOrderNumberFrom { get; set; }
        public int RangeOrderNumberTo { get; set; }
        public int OrderNumberForPrint { get { return _orderNumberForPrint; } }

        public string BarCodeValue { get; set; }
        public string LanguageTypeId { get; set; }

        public bool takeAway { get; set; }

        public DateTime? OrderDate { get; set; }

        private List<DishItem> _dishItems;

        public List<DishItem> Dishes { get { return _dishItems; } }


        // constructor
        public OrderItem()
        {
            _dishItems = new List<DishItem>();
            RangeOrderNumberFrom = 1; RangeOrderNumberTo = 9999;
        }

        // добавить КОПИЮ DishItem
        public void AddDish(DishItem dish)
        {
            if (dish != null)
            {
                _dishItems.Add(dish);
            }
        }
        public void RemoveDish(DishItem dish)
        {
            if (dish != null) _dishItems.Remove(dish);
        }

        private bool isExist(DishItem checkingItem)
        {
            return _dishItems.Any(d => d.RowGUID == checkingItem.RowGUID);
        }

        public decimal GetOrderValue()
        {
            decimal retVal = 0;
            if (this.Dishes != null)
                foreach (DishItem item in Dishes) retVal += item.GetValueInOrder();
            
            return retVal;
        }

        public int CreateOrderNumberForPrint(out DateTime? dtOrder)
        {
            int retVal;

            using (NoodleDContext db = new NoodleDContext())
            {
                Terminal term = db.Terminal.FirstOrDefault(t => t.Name == this.DeviceID);
                if (term == null)
                {
                    term = new Terminal() { Name = this.DeviceID, RndOrderNum_Date = DateTime.Now, TimeOn = DateTime.Now };
                    term.RndOrderNum_InitVal = getNewRandomOrderNumber(this.RangeOrderNumberFrom, this.RangeOrderNumberTo);
                    term.RndOrderNum_NextVal = term.RndOrderNum_InitVal;
                    db.Terminal.Add(term);
                }
                else
                {
                    if (term.RndOrderNum_InitVal == null)
                    {
                        term.RndOrderNum_Date = DateTime.Now;
                        term.RndOrderNum_InitVal = getNewRandomOrderNumber(this.RangeOrderNumberFrom, this.RangeOrderNumberTo);
                        term.RndOrderNum_NextVal = term.RndOrderNum_InitVal;
                    }
                    if (term.RndOrderNum_NextVal == null)
                    {
                        term.RndOrderNum_NextVal = term.RndOrderNum_InitVal;
                    }

                    // дата заказа
                    term.RndOrderNum_Date = DateTime.Now;
                    // для другой даты новый кеш случайных номеров заказа
                    if (compareDatesOnly(term.RndOrderNum_Date??DateTime.MinValue, DateTime.Now) == false)
                    {
                        term.RndOrderNum_InitVal = getNewRandomOrderNumber(this.RangeOrderNumberFrom, this.RangeOrderNumberTo);
                        term.RndOrderNum_NextVal = term.RndOrderNum_InitVal;
                    }
                }

                retVal = term.RndOrderNum_NextVal??-1;
                term.RndOrderNum_NextVal++;
                dtOrder = term.RndOrderNum_Date;

                db.SaveChanges();
            }

            _orderNumberForPrint = retVal;
            return retVal;
        }  // CreateOrderNumberForPrint
        private int getNewRandomOrderNumber(int rndFrom, int rndTo)
        {
            Random rndGen = new Random();
            return rndGen.Next(rndFrom, rndTo);
        }

        private bool compareDatesOnly(DateTime d1, DateTime d2)
        {
            return ((d1.Year == d2.Year) && (d1.Month == d2.Month) && (d1.Day == d2.Day));
        }

        public bool SaveToDB(out string errMsg)
        {
            bool retVal = true; errMsg = "";

            using (NoodleDContext db = new NoodleDContext())
            {
                Order newOrder = new Order()
                {
                    RowGUID = Guid.NewGuid(),
                    OrderNumForPrint = this.OrderNumberForPrint,
                    OrderDate = this.OrderDate??DateTime.MinValue,
                    BarCodeValue = this.BarCodeValue,
                    LanguageTypeId = this.LanguageTypeId,
                    takeAway = this.takeAway
                };
                db.Order.Add(newOrder);

                // добавить блюда
                if (this.Dishes != null)
                {
                    foreach (DishItem dItem in this.Dishes)
                    {
                        OrderDish oDish = new OrderDish()
                        {
                            OrderGUID = newOrder.RowGUID,
                            DishGUID = dItem.RowGUID,
                            Count = dItem.Count,
                            Price = dItem.Price
                        };
                        db.OrderDish.Add(oDish);
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception e)
                        {
                            errMsg = string.Format("Message: {0}\n\tStackTrace: {1}\n\tSource: {2}\n\tInnerException", e.Message, e.StackTrace, e.Source, e.InnerException.ToString());
                            retVal = false;
                        }

                        // гарниры
                        if ((dItem.SelectedGarnishes != null) && (dItem.SelectedGarnishes.Count > 0))
                        {
                            DishAdding garn = dItem.SelectedGarnishes[0];
                            //foreach (DishAdding garn in dItem.SelectedGarnishes)
                            //{
                            OrderDishGarnish dGarn = new OrderDishGarnish()
                                {
                                    OrderGUID = newOrder.RowGUID,
                                    DishGUID = dItem.RowGUID,
                                    GarnishGUID = garn.RowGUID,
                                    Count = garn.Count,
                                    Price = garn.Price,
                                    OrderDishId = oDish.Id
                                };
                                db.OrderDishGarnish.Add(dGarn);
                            //}
                        }
                        // ингредиенты
                        if (dItem.SelectedIngredients != null)
                        {
                            foreach (DishAdding ingr in dItem.SelectedIngredients)
                            {
                                OrderDishIngredient dIngr = new OrderDishIngredient()
                                {
                                    OrderGUID = newOrder.RowGUID,
                                    DishGUID = dItem.RowGUID,
                                    IngredientGUID = ingr.RowGUID,
                                    Count = ingr.Count,
                                    Price = ingr.Price,
                                    OrderDishId = oDish.Id
                                };
                                db.OrderDishIngredient.Add(dIngr);
                            }  // foreach
                        }  // if
                    }  // foreach
                }  // if

                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    errMsg = string.Format("Message: {0}\n\tStackTrace: {1}\n\tSource: {2}\n\tInnerException", e.Message, e.StackTrace, e.Source, e.InnerException.ToString());
                    retVal = false;
                }

            }

            return retVal;
        }  // method

        public void Clear()
        {
            this._dishItems.Clear();
        }

    } // class CurrentOrder

}
