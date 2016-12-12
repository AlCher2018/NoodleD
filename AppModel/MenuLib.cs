﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            foreach (MenuFolder item in
                from m in _listMenu where m.ParentId == 0 select m)
            {
                MenuItem mi = new MenuItem() { MenuFolder = item };
                mi.langNames = getLangTextDict(item.RowGUID, fieldTypeId);

                // добавить блюда к пункту меню
                mi.Dishes = getDishes(item.RowGUID, item.Id);

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
                retVal.Add(item.Lang, item.Value);
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
                var lGarn = _listGarn.Where(g => g.DishGUID == dishDb.RowGUID);
                if (lGarn.Count() > 0) {
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
                dishApp.Ingredients = new List<DishAdding>();
                foreach (DishIngredient item in _listIngr.Where(g => g.DishGUID == dishDb.RowGUID))
                {
                    DishAdding da = new DishAdding() { Id = item.Id, RowGUID = item.RowGUID, Price = item.Price };
                    da.langNames = getLangTextDict(item.RowGUID, 1);
                    dishApp.Ingredients.Add(da);
                }
                // маркеры
                dishApp.Marks = new List<DishAddingImage>();
                foreach (DishMarks item in _listMarks.Where(g => g.DishGUID == dishDb.RowGUID))
                {
                    DishMark dm = _listMark.FirstOrDefault(m => m.RowGUID == item.MarkGUID);
                    DishAddingImage da = new DishAddingImage() { Id = item.Id, Image = dm.Image };
                    da.langNames = getLangTextDict(dm.RowGUID, 1);
                    dishApp.Marks.Add(da);
                }
                // рекомендации
                dishApp.Recommends = new List<DishItem>();
                foreach (DishRecommends item in _listRecom.Where(g => g.DishGUID == dishDb.RowGUID))
                {
                    Dish recomDb = _listDish.FirstOrDefault(d => d.RowGUID == item.DishGUID);
                    if (recomDb != null)
                    {
                        DishItem itemRecom = getNewDishItem(recomDb);
                        dishApp.Recommends.Add(itemRecom);
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
        public Dictionary<string, string> langNames { get; set; }
        public Dictionary<string, string> langDescriptions { get; set; }
        public byte[] Image { get; set; }
        public int UnitCount { get; set; }
        public Dictionary<string, string> langUnitNames { get; set; }
        public decimal Price { get; set; }

        public List<DishAdding> Garnishes { get; set; }
        public List<DishAdding> Ingredients { get; set; }
        public List<DishAddingImage> Marks { get; set; }
        public List<DishItem> Recommends { get; set; }

        public List<DishAdding> SelectedGarnishes { get; set; }
        public List<DishAdding> SelectedIngredients { get; set; }


        // надписи на кнопках
        public Dictionary<string, string> langBtnSelGarnishText { get; set; }
        public Dictionary<string, string> langBtnAddDishText { get; set; }
    }  // class DishItem

    public class DishAdding
    {
        public int Id { get; set; }
        public Guid RowGUID { get; set; }
        public Dictionary<string, string> langNames { get; set; }
        public decimal Price { get; set; }
        public string Uid { get; set; }
    }

    public class DishAddingImage
    {
        public int Id { get; set; }
        public Guid RowGUID { get; set; }
        public Dictionary<string, string> langNames { get; set; }
        public byte[] Image { get; set; }
    }


}  // namespace AppModel