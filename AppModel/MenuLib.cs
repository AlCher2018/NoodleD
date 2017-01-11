using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;

namespace AppModel
{
    public class MenuLib
    {
        private static List<MenuFolder> _listMenu;
        private static List<Dish> _listDish;
        private static List<DishGarnish> _listGarn;
        private static List<DishIngredient> _listIngr;
        private static List<DishMarks> _listMarks;
        private static List<DishMark> _listMark;
        private static List<DishRecommends> _listRecom;
        private static List<StringValue> _listStrVal;
        private static List<DishUnit> _listUnit;
        private static List<FieldType> _listFldType;

        static MenuLib()
        {
            using (NoodleDContext db = new NoodleDContext())
            {
                db.Configuration.LazyLoadingEnabled = true;  // отключить ленивую загрузку
                _listMenu = db.MenuFolder.ToList();
                _listDish = db.Dish.ToList();
                _listStrVal = db.StringValue.ToList();
                _listFldType = db.FieldType.ToList();
                _listUnit = db.DishUnit.ToList();
                _listGarn = db.DishGarnish.ToList();
                _listIngr = db.DishIngredient.ToList();
                _listMark = db.DishMark.ToList();
                _listMarks = db.DishMarks.ToList();
                _listRecom = db.DishRecommends.ToList();
            }
        }

        //public static bool CheckDb()
        //{

        //    NoodleDContext db = new NoodleDContext();
            
        //}

        public static ObservableCollection<MenuItem> GetMenuMainFolders()
        {
            ObservableCollection<MenuItem> retVal = new ObservableCollection<MenuItem>();
            int fieldTypeId = 1;
            List<MenuFolder> lsort = (from m in _listMenu orderby m.RowPosition where m.ParentId == 0 select m).ToList();

            foreach (MenuFolder item in lsort)
            {
                MenuItem mi = new MenuItem() { MenuFolder = item };
                mi.langNames = getLangTextDict(item.RowGUID, fieldTypeId);

                // добавить блюда к пункту меню
                try
                {
                    mi.Dishes = getDishes(item.RowGUID, item.Id);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Ошибка доступа к данным в MenuLib.GetMenuMainFolders()\n" + e.Message + "\n" + e.StackTrace + "\nПрограмма будет закрыта.");
                    throw;
                }

                retVal.Add(mi);
            }
            if (retVal.Count == 0) retVal = null;

            return retVal;
        }  //  GetMenuMainFolders(string langId)

        public static Dictionary<string,string> getLangTextDict(Guid rowGuid, int fieldTypeId)
        {
            Dictionary<string, string> retVal = new Dictionary<string, string>();
            foreach (StringValue item in
                from val in _listStrVal where val.RowGUID == rowGuid && val.FieldType.Id == fieldTypeId select val)
            {
                if (retVal.Keys.Contains(item.Lang) == false) retVal.Add(item.Lang, item.Value);
            }
            return retVal;
        }

        // получить значение из StringVAlue
        private static string getLangStringValue(NoodleDContext db, Guid rowGuid, int fieldTypeId, string langId, string defaultValue=null)
        {
            string retVal = null;

            StringValue sValDb = db.StringValue.FirstOrDefault(v => v.RowGUID == rowGuid && v.FieldType.Id == fieldTypeId && v.Lang == langId);

            if (sValDb == null)
            {
                if (defaultValue != null) retVal = defaultValue;
            }
            else retVal = sValDb.Value;

            return retVal;
        }

        private static ObservableCollection<DishItem> getDishes(Guid menuGuid, int menuId)
        {
            ObservableCollection<DishItem> retVal = new ObservableCollection<DishItem>();

            foreach (Dish dishDb in
                    from d in _listDish where d.MenuFolderGUID == menuGuid orderby d.Id select d)
            {
                DishItem dishApp = getNewDishItem(dishDb);
                dishApp.MenuId = menuId;

                // гарниры
                var lGarn = _listGarn.Where(g => g.DishGUID == dishDb.RowGUID).ToList();
                if (lGarn.Count > 0) {
                    dishApp.Garnishes = new List<DishAdding>();
                    dishApp.SelectedGarnishes = new List<DishAdding>();
                    foreach (DishGarnish item in lGarn)
                    {
                        DishAdding da = new DishAdding() { Id = item.Id, RowGUID=item.RowGUID, Price = item.Price };
                        da.langNames = getLangTextDict(item.RowGUID, 1);
                        dishApp.Garnishes.Add(da);
                    }
                }

                // ингредиенты
                var lIngr = _listIngr.Where(g => g.DishGUID == dishDb.RowGUID).ToList();
                if (lIngr.Count > 0) {
                    dishApp.Ingredients = new List<DishAdding>();
                    foreach (DishIngredient item in lIngr)
                    {
                        DishAdding da = new DishAdding() { Id = item.Id, RowGUID = item.RowGUID, Price = item.Price, Image=item.Image };
                        da.langNames = getLangTextDict(item.RowGUID, 1);
                        dishApp.Ingredients.Add(da);
                    }
                }

                // маркеры
                var lMark = _listMarks.Where(g => g.DishGUID == dishDb.RowGUID).ToList();
                if (lMark.Count > 0)
                {
                    dishApp.Marks = new List<DishAddingImage>();
                    foreach (DishMarks item in lMark)
                    {
                        DishMark dm = _listMark.FirstOrDefault(m => m.RowGUID == item.MarkGUID);
                        DishAddingImage da = new DishAddingImage() { Id = item.Id, Image = dm.Image };
                        da.langNames = getLangTextDict(dm.RowGUID, 1);
                        dishApp.Marks.Add(da);
                    }
                }

                // рекомендации
                var lRecom = _listRecom.Where(g => g.DishGUID == dishDb.RowGUID).ToList();
                if (lRecom.Count > 0)
                {
                    dishApp.Recommends = new List<DishItem>();
                    foreach (DishRecommends item in lRecom)
                    {
                        Dish recomDb = _listDish.FirstOrDefault(d => d.RowGUID == item.RecommendGUID);
                        if (recomDb != null)
                        {
                            DishItem itemRecom = getNewDishItem(recomDb);
                            dishApp.Recommends.Add(itemRecom);
                        }
                    }
                }

                retVal.Add(dishApp);
            }  // foreach

            return retVal;
        }  // GetDishes(Guid menuGuid)

        private static DishItem getNewDishItem(Dish dishDb)
        {
            DishItem dishApp = new DishItem();
            dishApp.Id = dishDb.Id;
            dishApp.RowGUID = dishDb.RowGUID;
            dishApp.langNames = getLangTextDict(dishDb.RowGUID, 1);
            dishApp.langDescriptions = getLangTextDict(dishDb.RowGUID, 2);
            dishApp.Image = dishDb.Image;
            dishApp.UnitCount = dishDb.UnitCount ?? 0;
            // единица измерения
            if (dishDb.UnitGUID != null)
            {
                DishUnit du = _listUnit.FirstOrDefault(d => d.RowGUID == dishDb.UnitGUID);
                if (du != null) dishApp.langUnitNames = getLangTextDict(du.RowGUID, 3);
            }
            dishApp.Price = dishDb.Price ?? 0;

            return dishApp;
        }


    }  // class MenuLib

    public class MenuItem
    {
        public MenuFolder MenuFolder { get; set; }
        public Dictionary<string,string> langNames { get; set; }
        //public List<DishItem> Dishes { get; set; }
        public ObservableCollection<DishItem> Dishes { get; set; }
    }  // class MenuItem


    public class DishItem
    {
        public int MenuId { get; set; }
        public int Id { get; set; }
        public Guid RowGUID { get; set; }
        public int UnitCount { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }

        public Dictionary<string, string> langNames { get; set; }
        public Dictionary<string, string> langDescriptions { get; set; }
        public Dictionary<string, string> langUnitNames { get; set; }

        public byte[] Image { get; set; }

        public List<DishAdding> Garnishes { get; set; }
        public List<DishAdding> Ingredients { get; set; }
        public List<DishAddingImage> Marks { get; set; }
        public List<DishItem> Recommends { get; set; }

        public List<DishAdding> SelectedGarnishes { get; set; }
        public List<DishAdding> SelectedIngredients { get; set; }
        public List<DishItem> SelectedRecommends { get; set; }

        // надписи на кнопках
        public Dictionary<string, string> langBtnSelGarnishText { get; set; }
        public Dictionary<string, string> langBtnAddDishText { get; set; }

        public DishItem()
        {
            this.Count = 1;
        }

        public DishItem GetCopyForOrder()
        {
            DishItem other = (DishItem)this.MemberwiseClone();

            other.Count = this.Count;
            other.langDescriptions = this.langDescriptions;
            other.langNames = this.langNames;
            other.langUnitNames = this.langUnitNames;

            other.langBtnAddDishText = null;
            other.langBtnSelGarnishText = null;

            // скопировать гарниры
            if ((this.SelectedGarnishes != null) && (this.SelectedGarnishes.Count > 0))
            {
                other.SelectedGarnishes = new List<DishAdding>();
                foreach (DishAdding item in this.SelectedGarnishes) other.SelectedGarnishes.Add(item);
            }
            
            // ... ингредиенты
            if ((this.SelectedIngredients != null) && (this.SelectedIngredients.Count > 0))
            {
                other.SelectedIngredients = new List<DishAdding>();
                foreach (DishAdding item in this.SelectedIngredients) other.SelectedIngredients.Add(item);
            }

            // ... маркеры
            if ((this.Marks != null) && (this.Marks.Count > 0))
            {
                other.Marks = new List<DishAddingImage>();
                foreach (DishAddingImage item in this.Marks) other.Marks.Add(item);
            }

            return other;
        }

        // стоимость блюда (самого блюда или блюда с гарниром) вместе с ингредиентами
        // НО без заказанного количества!!!
        public decimal GetPrice()
        {
            decimal retVal = this.Price;
            if ((SelectedGarnishes != null) && (SelectedGarnishes.Count > 0))
                retVal = SelectedGarnishes[0].Price;
            // добавить ингредиенты
            if (SelectedIngredients != null) foreach (DishAdding item in this.SelectedIngredients) retVal += item.Price;

            return retVal;
        }
        // стоимость блюда В ЗАКАЗЕ (с учетом заказанного количества порций)
        public decimal GetValueInOrder()
        {
            return GetPrice()  * this.Count;
        }

    }  // class DishItem

    public class DishAdding
    {
        public int Id { get; set; }
        public Guid RowGUID { get; set; }
        public Dictionary<string, string> langNames { get; set; }
        public decimal Price { get; set; }
        public byte[] Image { get; set; }
        public string Uid { get; set; }
        public int Count { get; set; }
    }

    public class DishAddingImage
    {
        public int Id { get; set; }
        public Guid RowGUID { get; set; }
        public Dictionary<string, string> langNames { get; set; }
        public byte[] Image { get; set; }
        public int Count { get; set; }
    }


}  // namespace AppModel
