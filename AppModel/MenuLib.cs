using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;

namespace AppModel
{
    // статический класс-оболочка для обслуживания данных из БД
    public static class MenuLib
    {
        // статическая оболочка для получения меню
        public static List<MenuItem> GetMenuMainFolders()
        {
            List<MenuItem> retVal = null;
            using (MainMenu mm = new MainMenu())
            {
                retVal = mm.GetMenuItemsList();
            }

            return retVal;
        } 
    
    }  // class MenuLib


    // экземплярный класс главного меню
    public class MainMenu: IDisposable
    {
        NoodleDContext _db;
        List<StringValue> _stringTable;

        public MainMenu()
        {
            _db = new NoodleDContext();
            _db.Configuration.LazyLoadingEnabled = true;  // отключить ленивую загрузку

            _stringTable = _db.StringValue.ToList();
        }

        public List<MenuItem> GetMenuItemsList()
        {
            List<MenuItem> retVal = new List<MenuItem>();
            int fieldTypeId = 1;

            List<MenuFolder> lsort = (from m in _db.MenuFolder orderby m.RowPosition where m.ParentId == 0 select m).ToList();
            foreach (MenuFolder item in lsort)
            {
                MenuItem mi = new MenuItem() { MenuFolder = item };
                mi.langNames = getLangTextDict(item.RowGUID, fieldTypeId);

                // добавить блюда к пункту меню
                try
                {
                    mi.Dishes = getDishes(item);
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
        }  // GetMenuItemList

        private List<DishItem> getDishes(MenuFolder menuFolder)
        {
            Guid menuGuid = menuFolder.RowGUID;
            int menuId = menuFolder.Id;

            List<DishItem> retVal = new List<DishItem>();

            List<Dish> listDish = _db.Dish.ToList();
            List<DishGarnish> listGarn = _db.DishGarnish.ToList();
            List<DishMarks> listMarks = _db.DishMarks.ToList();
            List<DishMark> listMark = _db.DishMark.ToList();
            List<DishIngredient> listIngr = _db.DishIngredient.ToList();
            List<DishRecommends> listRecom = _db.DishRecommends.ToList();
            List<DishUnit> listUnit = _db.DishUnit.ToList();

            foreach (Dish dishDb in
                    from d in listDish where d.MenuFolderGUID == menuGuid orderby d.Id select d)
            {
                DishItem dishApp = getNewDishItem(dishDb, listUnit);
                dishApp.MenuId = menuId;

                // гарниры
                var lGarn = listGarn.Where(g => g.DishGUID == dishDb.RowGUID).ToList();
                if (lGarn.Count > 0)
                {
                    dishApp.Garnishes = new List<DishAdding>();
                    dishApp.SelectedGarnishes = new List<DishAdding>();
                    foreach (DishGarnish item in lGarn)
                    {
                        DishAdding da = new DishAdding() { Id = item.Id, RowGUID = item.RowGUID, Price = item.Price };
                        da.langNames = getLangTextDict(item.RowGUID, 1);
                        dishApp.Garnishes.Add(da);
                    }
                }

                // ингредиенты
                var lIngr = listIngr.Where(g => g.DishGUID == dishDb.RowGUID).ToList();
                if (lIngr.Count > 0)
                {
                    dishApp.Ingredients = new List<DishAdding>();
                    foreach (DishIngredient item in lIngr)
                    {
                        DishAdding da = new DishAdding() { Id = item.Id, RowGUID = item.RowGUID, Price = item.Price, Image = item.Image };
                        da.langNames = getLangTextDict(item.RowGUID, 1);
                        dishApp.Ingredients.Add(da);
                    }
                }

                // маркеры
                var lMark = listMarks.Where(g => g.DishGUID == dishDb.RowGUID).ToList();
                if (lMark.Count > 0)
                {
                    dishApp.Marks = new List<DishAdding>();
                    foreach (DishMarks item in lMark)
                    {
                        DishMark dm = listMark.FirstOrDefault(m => m.RowGUID == item.MarkGUID);
                        DishAdding da = new DishAdding() { Id = item.Id, Image = dm.Image };
                        da.langNames = getLangTextDict(dm.RowGUID, 1);
                        dishApp.Marks.Add(da);
                    }
                }

                // рекомендации
                var lRecom = listRecom.Where(g => g.DishGUID == dishDb.RowGUID).ToList();
                if (lRecom.Count > 0)
                {
                    dishApp.Recommends = new List<DishItem>();
                    foreach (DishRecommends item in lRecom)
                    {
                        Dish recomDb = listDish.FirstOrDefault(d => d.RowGUID == item.RecommendGUID);
                        if (recomDb != null)
                        {
                            DishItem itemRecom = getNewDishItem(recomDb, listUnit);
                            dishApp.Recommends.Add(itemRecom);
                        }
                    }
                }

                retVal.Add(dishApp);
            }  // foreach

            return retVal;
        }  // GetDishes(Guid menuGuid)

        private DishItem getNewDishItem(Dish dishDb, List<DishUnit> listUnit)
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
                DishUnit du = listUnit.FirstOrDefault(d => d.RowGUID == dishDb.UnitGUID);
                if (du != null) dishApp.langUnitNames = getLangTextDict(du.RowGUID, 3);
            }
            dishApp.Price = dishDb.Price ?? 0;

            return dishApp;
        }

        private Dictionary<string, string> getLangTextDict(Guid rowGuid, int fieldTypeId)
        {
            Dictionary<string, string> retVal = new Dictionary<string, string>();
            foreach (StringValue item in
                from val in _stringTable where val.RowGUID == rowGuid && val.FieldType.Id == fieldTypeId select val)
            {
                if (retVal.Keys.Contains(item.Lang) == false) retVal.Add(item.Lang, item.Value);
            }
            return retVal;
        }


        public void Dispose()
        {
           if (_db != null)
            {
                _stringTable = null;
                _db.Dispose(); _db = null;
            }
        }
    }

    public class MenuItem : INotifyPropertyChanged
    {
        private MenuFolder _menuFolder;
        private Dictionary<string, string> _langNames;
        private List<DishItem> _dishes;

        public MenuFolder MenuFolder
        {
            get { return _menuFolder; }
            set
            {
                if (value == _menuFolder) return;
                _menuFolder = value;
                NotifyPropertyChanged();
            }
        }

        public Dictionary<string,string> langNames
        {
            get { return _langNames; }
            set
            {
                if (value == _langNames) return;
                _langNames = value;
                NotifyPropertyChanged();
            }
        }

        public List<DishItem> Dishes
        {
            get { return _dishes; }
            set
            {
                if (value == _dishes) return;
                _dishes = value;
                NotifyPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


    }  // class MenuItem

    public class DishItem : INotifyPropertyChanged
    {
        private int _unitCount;
        private decimal _price;
        private int _count;
        private Dictionary<string, string> _langNames;
        private Dictionary<string, string> _langDescription;
        private Dictionary<string, string> _langUnitNames;
        private byte[] _image;
        private List<DishAdding> _garnishes;
        private List<DishAdding> _ingredients;
        private List<DishAdding> _selectedGarnishes;
        private List<DishAdding> _selectedIngredients;
        private List<DishAdding> _marks;
        private List<DishItem> _recommends;
        private List<DishItem> _selectedRecommends;


        public int MenuId { get; set; }
        public int Id { get; set; }
        public Guid RowGUID { get; set; }
        public int UnitCount
        {
            get { return _unitCount; }
            set
            {
                if (value == _unitCount) return;
                _unitCount = value;
                NotifyPropertyChanged();
            }
        }
        public decimal Price
        {
            get { return _price; }
            set
            {
                if (value == _price) return;
                _price = value;
                NotifyPropertyChanged();
            }
        }
        public int Count
        {
            get { return _count; }
            set
            {
                if (value == _count) return;
                _count = value;
                NotifyPropertyChanged();
            }
        }

        public Dictionary<string, string> langNames
        {
            get { return _langNames; }
            set
            {
                if (value == _langNames) return;
                _langNames = value;
                NotifyPropertyChanged();
            }
        }
        public Dictionary<string, string> langDescriptions
        {
            get { return _langDescription; }
            set
            {
                if (value == _langDescription) return;
                _langDescription = value;
                NotifyPropertyChanged();
            }
        }
        public Dictionary<string, string> langUnitNames
        {
            get { return _langUnitNames; }
            set
            {
                if (value == _langUnitNames) return;
                _langUnitNames = value;
                NotifyPropertyChanged();
            }
        }

        public byte[] Image
        {
            get { return _image; }
            set
            {
                if (value == _image) return;
                _image = value;
                NotifyPropertyChanged();
            }
        }

        public List<DishAdding> Garnishes
        {
            get { return _garnishes; }
            set
            {
                if (value == _garnishes) return;
                _garnishes = value;
                NotifyPropertyChanged();
            }
        }
        public List<DishAdding> Ingredients
        {
            get { return _ingredients; }
            set
            {
                if (value == _ingredients) return;
                _ingredients = value;
                NotifyPropertyChanged();
            }
        }
        public List<DishAdding> Marks
        {
            get { return _marks; }
            set
            {
                if (value == _marks) return;
                _marks = value;
                NotifyPropertyChanged();
            }
        }
        public List<DishItem> Recommends
        {
            get { return _recommends; }
            set
            {
                if (value == _recommends) return;
                _recommends = value;
                NotifyPropertyChanged();
            }
        }

        public List<DishAdding> SelectedGarnishes
        {
            get { return _selectedGarnishes; }
            set
            {
                if (value == _selectedGarnishes) return;
                _selectedGarnishes = value;
                NotifyPropertyChanged();
            }
        }
        public List<DishAdding> SelectedIngredients
        {
            get { return _selectedIngredients; }
            set
            {
                if (value == _selectedIngredients) return;
                _selectedIngredients = value;
                NotifyPropertyChanged();
            }
        }
        public List<DishItem> SelectedRecommends
        {
            get { return _selectedRecommends; }
            set
            {
                if (value == _selectedRecommends) return;
                _selectedRecommends = value;
                NotifyPropertyChanged();
            }
        }

        public void ClearAllSelections()
        {
            if (this.SelectedGarnishes != null)     clearList<DishAdding>(this.SelectedGarnishes);
            if (this.SelectedIngredients != null)   clearList<DishAdding>(this.SelectedIngredients);
            if (this.SelectedRecommends != null)    clearList<DishItem>(this.SelectedRecommends);
        }
        private void clearList<T>(List<T> list)
        {
            list.RemoveAll(d => true);
            list = null;
        }

        public DishItem()
        {
            this.Count = 1;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public DishItem GetCopyForOrder()
        {
            DishItem other = (DishItem)this.MemberwiseClone();

            other.Count = this.Count;
            other.langDescriptions = this.langDescriptions;
            other.langNames = this.langNames;
            other.langUnitNames = this.langUnitNames;

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
                other.Marks = new List<DishAdding>();
                foreach (DishAdding item in this.Marks) other.Marks.Add(item);
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

    public class DishAdding : INotifyPropertyChanged
    {
        private Dictionary<string, string> _langNames;
        private decimal _price;
        private byte[] _image;
        private int _count;

        public int Id { get; set; }
        public Guid RowGUID { get; set; }
        public Dictionary<string, string> langNames
        {
            get { return _langNames; }
            set
            {
                if (value == _langNames) return;
                _langNames = value;
                NotifyPropertyChanged();
            }
        }
        public decimal Price
        {
            get { return _price; }
            set
            {
                if (value == _price) return;
                _price = value;
                NotifyPropertyChanged();
            }
        }
        public byte[] Image
        {
            get { return _image; }
            set
            {
                if (value == _image) return;
                _image = value;
                NotifyPropertyChanged();
            }
        }
        public string Uid { get; set; }
        public int Count
        {
            get { return _count ; }
            set
            {
                if (value == _count) return;
                _count = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }

}  // namespace AppModel
