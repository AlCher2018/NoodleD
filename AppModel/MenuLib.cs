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
using System.Windows.Media.Imaging;

namespace AppModel
{
    // статический класс-оболочка для обслуживания данных из БД
    public static class MenuLib
    {
        public static Action<string> MenuFolderHandler;
        // статическая оболочка для получения меню
        public static List<MenuItem> GetMenuMainFolders()
        {
            List<MenuItem> retVal = null;
            using (MainMenu mm = new MainMenu())
            {
                if (MenuLib.MenuFolderHandler != null) mm.MenuFolderHandler = MenuLib.MenuFolderHandler;
                retVal = mm.GetMenuItemsList();
            }

            return retVal;
        } 
    
    }  // class MenuLib


    // экземплярный класс главного меню
    public class MainMenu: IDisposable
    {
        private NoodleDContext _db;
        private List<StringValue> _stringTable;

        public Action<string> MenuFolderHandler;

        public MainMenu()
        {
            _db = new NoodleDContext();
            _db.Configuration.LazyLoadingEnabled = true;  // отключить ленивую загрузку

            _stringTable = _db.StringValue.ToList();
        }

        public List<MenuItem> GetMenuItemsList()
        {
            List<MenuItem> retVal = new List<MenuItem>();
            FieldTypeIDEnum fieldTypeId = FieldTypeIDEnum.Name;

            List<MenuFolder> lsort = (from m in _db.MenuFolder orderby m.RowPosition where m.ParentId == 0 select m).ToList();
            foreach (MenuFolder item in lsort)
            {
                MenuItem newMenuItem = new MenuItem() { MenuFolder = item };
                newMenuItem.langNames = getLangTextDict(item.RowGUID, fieldTypeId);
                // передать в обработчик наименование пункта меню
                if (MenuFolderHandler != null)
                {
                    string folderName = getFolderNameRu(item.RowGUID);
                    if (folderName != null) MenuFolderHandler.Invoke(folderName);
                }

                // добавить блюда к пункту меню
                try
                {
                    newMenuItem.Dishes = getDishes(item);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Ошибка доступа к данным в MenuLib.GetMenuMainFolders()\n" + e.Message + "\n" + e.StackTrace + "\nПрограмма будет закрыта.");
                    throw;
                }

                retVal.Add(newMenuItem);
            }

            if (retVal.Count == 0) retVal = null;

            return retVal;
        }  // GetMenuItemList

        private string getFolderNameRu(Guid rowGUID)
        {
            StringValue row = _stringTable.FirstOrDefault(r => (r.RowGUID==rowGUID) && (r.FieldTypeId==1) && (r.Lang=="ru"));
            if (row == null)
                return null;
            else
                return row.Value;
        }

        private List<DishItem> getDishes(MenuFolder menuFolder)
        {
            Guid menuGuid = menuFolder.RowGUID;
            int menuId = menuFolder.Id;

            List<DishItem> retVal = new List<DishItem>();

            List<Dish> listDish = _db.Dish.ToList();
            List<Dish> sortedDish = (from d in listDish where d.MenuFolderGUID == menuGuid orderby d.RowPosition select d).ToList();
            List<DishGarnish> listGarn = _db.DishGarnish.ToList();
            List<DishMarks> listMarks = _db.DishMarks.ToList();
            List<DishMark> listMark = _db.DishMark.ToList();
            List<DishIngredient> listIngr = _db.DishIngredient.ToList();
            List<DishRecommends> listRecom = _db.DishRecommends.ToList();
            List<DishUnit> listUnit = _db.DishUnit.ToList();

            foreach (Dish dishDb in sortedDish)
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
                        DishAdding da = new DishAdding() {
                            Id = item.Id, RowGUID = item.RowGUID, Price = item.Price,
                            Image = ImageHelper.ByteArrayToBitmapImage(item.Image),
                            ImageDish = ImageHelper.ByteArrayToBitmapImage(item.ImageDishWithGarnish),
                            langNames = getLangTextDict(item.RowGUID, FieldTypeIDEnum.Name), 
                            langDishDescr = getLangTextDict(item.RowGUID, FieldTypeIDEnum.Description)
                        };
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
                        DishAdding da = new DishAdding()
                        {
                            Id = item.Id, RowGUID = item.RowGUID, Price = item.Price,
                            Image = ImageHelper.ByteArrayToBitmapImage(item.Image)
                        };
                        da.langNames = getLangTextDict(item.RowGUID, FieldTypeIDEnum.Name);
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
                        DishAdding da = new DishAdding()
                        {
                            Id = item.Id,
                            Image = ImageHelper.ByteArrayToBitmapImage(dm.Image)
                        };
                        da.langNames = getLangTextDict(dm.RowGUID, FieldTypeIDEnum.Name);
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
                        Dish recomDb = _db.Dish.FirstOrDefault(d => d.RowGUID == item.RecommendGUID);
                        if (recomDb != null)
                        {
                            DishItem itemRecom = getNewDishItem(recomDb, listUnit);
                            dishApp.Recommends.Add(itemRecom);
                        }
                    }
                }

                retVal.Add(dishApp);
            }  // foreach

            GC.Collect();
            listGarn.Clear(); listGarn = null;
            listMarks.Clear(); listMarks = null;
            listMark.Clear(); listMark = null;
            listIngr.Clear(); listIngr = null;
            listRecom.Clear(); listRecom = null;
            listUnit.Clear(); listUnit = null;
            sortedDish.Clear(); sortedDish = null;
            listDish.Clear(); listDish = null;
            GC.Collect();

            return retVal;
        }  // GetDishes(Guid menuGuid)

        private DishItem getNewDishItem(Dish dishDb, List<DishUnit> listUnit)
        {
            DishItem dishApp = new DishItem();
            dishApp.Id = dishDb.Id;
            dishApp.RowGUID = dishDb.RowGUID;
            dishApp.langNames = getLangTextDict(dishDb.RowGUID, FieldTypeIDEnum.Name);
            dishApp.langDescriptions = getLangTextDict(dishDb.RowGUID, FieldTypeIDEnum.Description);

            GC.Collect();
            var v1 = GC.GetTotalMemory(true);
            dishApp.Image = ImageHelper.ByteArrayToBitmapImage(dishDb.Image);
            var v2 = GC.GetTotalMemory(true);

            dishApp.UnitCount = dishDb.UnitCount ?? 0;
            // единица измерения
            if (dishDb.UnitGUID != null)
            {
                DishUnit du = listUnit.FirstOrDefault(d => d.RowGUID == dishDb.UnitGUID);
                if (du != null) dishApp.langUnitNames = getLangTextDict(du.RowGUID, FieldTypeIDEnum.UnitName);
            }
            dishApp.Price = dishDb.Price ?? 0;

            return dishApp;
        }

        private Dictionary<string, string> getLangTextDict(Guid rowGuid, FieldTypeIDEnum fieldTypeId)
        {
            Dictionary<string, string> retVal = new Dictionary<string, string>();
            foreach (StringValue item in
                from val in _stringTable where val.RowGUID == rowGuid && val.FieldType.Id == (int)fieldTypeId select val)
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

    public class MenuItem
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
            }
        }

        public Dictionary<string,string> langNames
        {
            get { return _langNames; }
            set
            {
                if (value == _langNames) return;
                _langNames = value;
            }
        }

        public List<DishItem> Dishes
        {
            get { return _dishes; }
            set
            {
                if (value == _dishes) return;
                _dishes = value;
            }
        }
    }  // class MenuItem

    public class DishItem
    {
        private Dictionary<string, string> _langNames;
        private Dictionary<string, string> _langDescription;
        private Dictionary<string, string> _langUnitNames;
        private BitmapImage _image;
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
        public int UnitCount { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }

        public Dictionary<string, string> langNames
        {
            get { return _langNames; }
            set
            {
                if (value == _langNames) return;
                _langNames = value;
            }
        }
        public Dictionary<string, string> langDescriptions
        {
            get { return _langDescription; }
            set
            {
                if (value == _langDescription) return;
                _langDescription = value;
            }
        }
        public Dictionary<string, string> langUnitNames
        {
            get { return _langUnitNames; }
            set
            {
                if (value == _langUnitNames) return;
                _langUnitNames = value;
            }
        }

        public BitmapImage Image
        {
            get { return _image; }
            set
            {
                if (value == _image) return;
                _image = value;
            }
        }

        public List<DishAdding> Garnishes
        {
            get { return _garnishes; }
            set
            {
                if (value == _garnishes) return;
                _garnishes = value;
            }
        }
        public List<DishAdding> Ingredients
        {
            get { return _ingredients; }
            set
            {
                if (value == _ingredients) return;
                _ingredients = value;
            }
        }
        public List<DishAdding> Marks
        {
            get { return _marks; }
            set
            {
                if (value == _marks) return;
                _marks = value;
            }
        }
        public List<DishItem> Recommends
        {
            get { return _recommends; }
            set
            {
                if (value == _recommends) return;
                _recommends = value;
            }
        }

        public List<DishAdding> SelectedGarnishes
        {
            get { return _selectedGarnishes; }
            set
            {
                if (value == _selectedGarnishes) return;
                _selectedGarnishes = value;
            }
        }
        public List<DishAdding> SelectedIngredients
        {
            get { return _selectedIngredients; }
            set
            {
                if (value == _selectedIngredients) return;
                _selectedIngredients = value;
            }
        }
        public List<DishItem> SelectedRecommends
        {
            get { return _selectedRecommends; }
            set
            {
                if (value == _selectedRecommends) return;
                _selectedRecommends = value;
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
        private BitmapImage _image;
        private int _count;

        public int Id { get; set; }
        public Guid RowGUID { get; set; }

        // ONLY adding name (only adding (short) or dish with adding (full))
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
        public Dictionary<string, string> langDishDescr { get; set; }   // dish with adding description

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
        public BitmapImage Image
        {
            get { return _image; }
            set
            {
                if (value == _image) return;
                _image = value;
                NotifyPropertyChanged();
            }
        }                   // ONLY adding image
        public BitmapImage ImageDish { get; set; }   // dish WITH adding image
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

    } // class DishAdding

    public enum FieldTypeIDEnum
    {
        Name =1, Description = 2, UnitName = 3
    }

}  // namespace AppModel
