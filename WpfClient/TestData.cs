using AppModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WpfClient
{
    public class TestData
    {
        public static void mainProc()
        {
            //clearData();

            //setGlobalDataFrom1C();

            //setMainMenu();
            //setDishUnits();
            //setMarkerDict();

            //setDishesTestData();
            //setRecommends();
        }

        private static void clearData()
        {
//select* from DishIngredient
//select* from DishGarnish
//select* from DishMarks
//select* from Dish


//delete from DishIngredient
//delete from DishGarnish
//delete from DishMarks
//delete from Dish

//dbcc checkident('DishIngredient', reseed, 0)
//dbcc checkident('DishGarnish', reseed, 0)
//dbcc checkident('DishMarks', reseed, 0)
//dbcc checkident('Dish', reseed, 0)


            using (NoodleDContext db = new NoodleDContext())
            {
                db.Setting.RemoveRange(db.Setting);
                db.StringValue.RemoveRange(db.StringValue);
                db.MenuFolder.RemoveRange(db.MenuFolder);
                db.DishUnit.RemoveRange(db.DishUnit);
                db.DishRecommends.RemoveRange(db.DishRecommends);
                db.DishIngredient.RemoveRange(db.DishIngredient);
                db.DishGarnish.RemoveRange(db.DishGarnish);
                db.DishMarks.RemoveRange(db.DishMarks);
                db.DishMark.RemoveRange(db.DishMark);
                db.Dish.RemoveRange(db.Dish);

                db.SaveChanges();

                // сбросить счетчики
                db.Database.ExecuteSqlCommand("dbcc checkident('[StringValue]', reseed, 0)");
                db.Database.ExecuteSqlCommand("dbcc checkident('[MenuFolder]', reseed, 0)");
                db.Database.ExecuteSqlCommand("dbcc checkident('[DishUnit]', reseed, 0)");
                db.Database.ExecuteSqlCommand("dbcc checkident('[DishRecommends]', reseed, 0)");
                db.Database.ExecuteSqlCommand("dbcc checkident('[DishIngredient]', reseed, 0)");
                db.Database.ExecuteSqlCommand("dbcc checkident('[DishGarnish]', reseed, 0)");
                db.Database.ExecuteSqlCommand("dbcc checkident('[DishMark]', reseed, 0)");
                db.Database.ExecuteSqlCommand("dbcc checkident('[DishMarks]', reseed, 0)");
                db.Database.ExecuteSqlCommand("dbcc checkident('[Dish]', reseed, 0)");
            }
        }

        public static void setGlobalDataFrom1C()
        {
            using (NoodleDContext db = new NoodleDContext())
            {
                Setting appSet;
                // главное окно
                appSet = new Setting() { UniqName = "appBackgroundColor", RowGUID = Guid.NewGuid(), Value = "122,34,104" };
                db.Setting.Add(appSet);
                db.SaveChanges();
                appSet = new Setting() { UniqName = "appNotSelectedItemColor", RowGUID = Guid.NewGuid(), Value = "255,255,255" };
                db.Setting.Add(appSet);
                appSet = new Setting() { UniqName = "appSelectedItemColor", RowGUID = Guid.NewGuid(), Value = "255,200,62" };
                db.Setting.Add(appSet);
                appSet = new Setting() { UniqName = "mainMenuSelectedItemColor", RowGUID = Guid.NewGuid(), Value = "99,29,85" };
                db.Setting.Add(appSet);
                appSet = new Setting() { UniqName = "addButtonBackgroundTextColor", RowGUID = Guid.NewGuid(), Value = "173,32,72" };
                db.Setting.Add(appSet);

                appSet = new Setting() { UniqName = "langButtonTextUa", RowGUID = Guid.NewGuid(), Value = "Укр" };
                db.Setting.Add(appSet);
                appSet = new Setting() { UniqName = "langButtonTextRu", RowGUID = Guid.NewGuid(), Value = "Рус" };
                db.Setting.Add(appSet);
                appSet = new Setting() { UniqName = "langButtonTextEn", RowGUID = Guid.NewGuid(), Value = "Eng" };
                db.Setting.Add(appSet);

                appSet = new Setting() { UniqName = "langButtonDefaultId", RowGUID = Guid.NewGuid(), Value = "ru" };
                db.Setting.Add(appSet);

                appSet = new Setting() { UniqName = "invitePromoText", RowGUID = Guid.NewGuid(), Value = "StringValue" };
                LangStringLib.SetValues(db, appSet.RowGUID, 1, "Введите промо-код", "Введіть промо-код", "Enter the promo code");
                db.Setting.Add(appSet);
                appSet = new Setting() { UniqName = "btnCreateOrderText", RowGUID = Guid.NewGuid(), Value = "StringValue" };
                LangStringLib.SetValues(db, appSet.RowGUID, 1, "ОФОРМИТЬ", "ОФОРМИТИ", "MAKE");
                db.Setting.Add(appSet);
                appSet = new Setting() { UniqName = "btnSelectGarnishText", RowGUID = Guid.NewGuid(), Value = "StringValue" };
                LangStringLib.SetValues(db, appSet.RowGUID, 1, "ВЫБЕРИТЕ ОСНОВУ", "ВИБЕРІТЬ ОСНОВУ", "SELECT BASE");
                db.Setting.Add(appSet);
                appSet = new Setting() { UniqName = "btnSelectDishText", RowGUID = Guid.NewGuid(), Value = "StringValue" };
                LangStringLib.SetValues(db, appSet.RowGUID, 1, "ДОБАВИТЬ", "ДОДАТИ", "ADD");
                db.Setting.Add(appSet);

                // всплывашка
                appSet = new Setting() { UniqName = "formPopUpHeaderText1", RowGUID = Guid.NewGuid(), Value = "StringValue" };
                LangStringLib.SetValues(db, appSet.RowGUID, 1, "Вы выбрали блюдо", "Ви вибрали блюдо", "You have chosen a dish");
                db.Setting.Add(appSet);

                appSet = new Setting() { UniqName = "formPopUpHeaderText2", RowGUID = Guid.NewGuid(), Value = "StringValue" };
                LangStringLib.SetValues(db, appSet.RowGUID, 1, "Удвоить ингредиенты", "Подвоїти інгредієнти", "Double the ingredients");
                db.Setting.Add(appSet);

                appSet = new Setting() { UniqName = "formPopUpHeaderText3", RowGUID = Guid.NewGuid(), Value = "StringValue" };
                LangStringLib.SetValues(db, appSet.RowGUID, 1, "Рекомендуем к этому блюду", "Рекомендуємо до цієї страви", "We recommend this dish");
                db.Setting.Add(appSet);

                // корзина
                appSet = new Setting() { UniqName = "formOrderHeaderText", RowGUID = Guid.NewGuid(), Value = "StringValue" };
                LangStringLib.SetValues(db, appSet.RowGUID, 1, "Вы заказали", "Ви замовили", "You ordered");
                db.Setting.Add(appSet);

                appSet = new Setting() { UniqName = "btnBackToMenuText", RowGUID = Guid.NewGuid(), Value = "StringValue" };
                LangStringLib.SetValues(db, appSet.RowGUID, 1, "Назад к меню", "Назад до меню", "Back to the menu");
                db.Setting.Add(appSet);

                appSet = new Setting() { UniqName = "btnTakeAwayText", RowGUID = Guid.NewGuid(), Value = "StringValue" };
                LangStringLib.SetValues(db, appSet.RowGUID, 1, "С собой", "З собою", "Take away");
                db.Setting.Add(appSet);

                appSet = new Setting() { UniqName = "btnPrintBillText", RowGUID = Guid.NewGuid(), Value = "StringValue" };
                LangStringLib.SetValues(db, appSet.RowGUID, 1, "Распечатать чек", "Роздрукувати чек", "Print receipt");
                db.Setting.Add(appSet);

                appSet = new Setting() { UniqName = "lblTotalText", RowGUID = Guid.NewGuid(), Value = "StringValue" };
                LangStringLib.SetValues(db, appSet.RowGUID, 1, "Всего:", "Всього:", "Total:");
                db.Setting.Add(appSet);

                appSet = new Setting() { UniqName = "lblGoText", RowGUID = Guid.NewGuid(), Value = "StringValue" };
                LangStringLib.SetValues(db, appSet.RowGUID, 1, "Подходите с чеком к кассе для оплаты", "Підходьте з чеком до каси для оплати", "Come with a check to the cashier for payment");
                db.Setting.Add(appSet);

                db.SaveChanges();
            }
        }


        public static void setMainMenu()
        {
            MenuFolder mf1, mf2, mf3, mf4, mf5, mf6;
            using (NoodleDContext db = new NoodleDContext())
            {
                byte[] image = AppLib.getImageFromFilePath("AppImages\\menuItemNoImage.png");
                mf1 = new MenuFolder() { RowGUID = Guid.NewGuid(), HasIngredients = true, Image = image, ParentId = 0, RowPosition = 1 };
                mf2 = new MenuFolder() { RowGUID = Guid.NewGuid(), HasIngredients = false, Image = image, ParentId = 0, RowPosition = 2 };
                mf3 = new MenuFolder() { RowGUID = Guid.NewGuid(), HasIngredients = false, Image = image, ParentId = 0, RowPosition = 3 };
                mf4 = new MenuFolder() { RowGUID = Guid.NewGuid(), HasIngredients = false, Image = image, ParentId = 0, RowPosition = 4 };
                mf5 = new MenuFolder() { RowGUID = Guid.NewGuid(), HasIngredients = false, Image = image, ParentId = 0, RowPosition = 5 };
                mf6 = new MenuFolder() { RowGUID = Guid.NewGuid(), HasIngredients = false, Image = image, ParentId = 0, RowPosition = 6 };
                db.MenuFolder.AddRange(new[] { mf1, mf2, mf3, mf4, mf5, mf6 });

                db.SaveChanges();

                LangStringLib.SetValues(db, mf1.RowGUID, 1, "Комбо", "Комбо", "Combo");
                LangStringLib.SetValues(db, mf2.RowGUID, 1, "Воки", "Вокi", "Vocki");
                LangStringLib.SetValues(db, mf3.RowGUID, 1, "Салаты", "Салати", "Salads");
                LangStringLib.SetValues(db, mf4.RowGUID, 1, "Супы", "Супи", "Soups");
                LangStringLib.SetValues(db, mf5.RowGUID, 1, "Десерты", "Десерти", "Swits");
                LangStringLib.SetValues(db, mf6.RowGUID, 1, "Напитки", "Напої", "Beverages");
                db.SaveChanges();
            }
        }

        public static void setDishUnits()
        {
            using (NoodleDContext db = new NoodleDContext())
            {
                DishUnit unit;
                
                // гр
                unit = new DishUnit() { RowGUID = Guid.NewGuid() };
                db.DishUnit.Add(unit);
                LangStringLib.SetValues(db, unit.RowGUID, 3, "г", "г", "gr");

                // мл
                unit = new DishUnit() { RowGUID = Guid.NewGuid() };
                db.DishUnit.Add(unit);
                LangStringLib.SetValues(db, unit.RowGUID, 3, "мл", "мл", "ml");

                db.SaveChanges();
            }
        }

        public static void setMarkerDict()
        {
            using (NoodleDContext db = new NoodleDContext())
            {
                // добавить маркеры
                DishMark dm;
                dm = new DishMark() { RowGUID = Guid.NewGuid(), Image = AppLib.getImageFromFilePath(@"e:\Чернов\NoodleD\images\chilli.jpg") };
                db.DishMark.Add(dm);
                LangStringLib.SetValues(db, dm.RowGUID, 1, "острый","гострий","chilli");

                dm = new DishMark() { RowGUID = Guid.NewGuid(), Image = AppLib.getImageFromFilePath(@"e:\Чернов\NoodleD\images\Red_Apple.jpg") };
                LangStringLib.SetValues(db, dm.RowGUID, 1, "вег", "вег", "veg");
                db.DishMark.Add(dm);

                db.SaveChanges();
            }
        }


        public static void setDishesTestData()
        {
            MenuFolder m1;
            int unitCount; decimal dishPrice; string imagePath;
            int menuItemId, cnt;
            string[] namesList, descrList;
            Guid dUnitGuid1, dUnitGuid2;
            Guid dMarkGuid1, dMarkGuid2;

            // блюда для первого пункта меню (Комбо)
            namesList = new string[] { "Курица Кунпао", "Куриця Кунпао", "Cunpao chicken" };
            descrList = new string[] { "Перец болгарский, фасоль стручковая, перец чили, арахис, лук зеленый", "Перець болгарський, фасоль стручкова, перець чiлi, арахiс, лук зелений", "Bell peppers, green beans, chili, peanuts, green onion" };
            // addDishes(1, 7, descrList, descrList, 1, 400, 30, @"e:\Чернов\NoodleD\images\dish2.jpg");
            menuItemId = 1; cnt = 7; unitCount = 400; dishPrice=30; 
            imagePath = @"e:\Чернов\NoodleD\images\dish2.jpg";
            using (NoodleDContext db = new NoodleDContext())
            {
                // единицы измерения
                dUnitGuid1 = db.DishUnit.ToList().ElementAt(0).RowGUID;  // гр
                dUnitGuid2 = db.DishUnit.ToList().ElementAt(1).RowGUID;  // мл
                // маркеры
                dMarkGuid1 = db.DishMark.ToList().ElementAt(0).RowGUID;  // перчик
                dMarkGuid2 = db.DishMark.ToList().ElementAt(1).RowGUID;  // яблочко

                Random rnd = new Random();

                m1 = db.MenuFolder.First(m => m.Id == menuItemId);
                for (int i = 0; i < cnt; i++)
                {
                    Dish dish = new Dish() { MenuFolderGUID = m1.RowGUID, RowGUID = Guid.NewGuid(),
                        UnitCount = unitCount, UnitGUID = dUnitGuid1, Price = dishPrice, RowPosition=i };
                    dish.Image = AppLib.getImageFromFilePath(imagePath);
                    db.Dish.Add(dish);
                    db.SaveChanges();

                    // добавить к блюду маркеры
                    int r = rnd.Next(1, 4);
                    if (r == 2) db.DishMarks.Add(new DishMarks() { DishGUID = dish.RowGUID, MarkGUID = dMarkGuid1 });
                    else if (r == 3) db.DishMarks.Add(new DishMarks() { DishGUID = dish.RowGUID, MarkGUID = dMarkGuid2 });
                    else if (r == 4)
                    {
                        db.DishMarks.Add(new DishMarks() { DishGUID = dish.RowGUID, MarkGUID = dMarkGuid1 });
                        db.DishMarks.Add(new DishMarks() { DishGUID = dish.RowGUID, MarkGUID = dMarkGuid2 });
                    }
                    // добавить к блюду ВСЕ ингредиенты
                    addIngredients(db, dish);
                    // добавить к блюду ВСЕ гарниры
                    addGarnishes(db, dish);
                    db.SaveChanges();

                    // наименование
                    LangStringLib.SetValues(db, dish.RowGUID, 1, namesList[0], namesList[1], namesList[2]);
                    // описание
                    LangStringLib.SetValues(db, dish.RowGUID, 2, descrList[0], descrList[1], descrList[2]);
                }
            }

            // блюда для второго пункта меню (Воки)
            //addDishes(2, 3, l1, l2, 1, 300, 30, @"e:\Чернов\NoodleD\images\dish2.jpg");
            menuItemId = 2; cnt = 3; unitCount = 300; dishPrice = 30;
            imagePath = @"e:\Чернов\NoodleD\images\dish2.jpg";
            using (NoodleDContext db = new NoodleDContext())
            {
                m1 = db.MenuFolder.First(m => m.Id == menuItemId);
                DishUnit dUnit = db.DishUnit.ToList().ElementAt(0);  // гр

                for (int i = 0; i < cnt; i++)
                {
                    Dish dish = new Dish() { MenuFolderGUID = m1.RowGUID, RowGUID = Guid.NewGuid(),
                        UnitCount = unitCount, UnitGUID = dUnitGuid1, Price = dishPrice, RowPosition = i };
                    dish.Image = AppLib.getImageFromFilePath(imagePath);
                    db.Dish.Add(dish);
                    db.SaveChanges();

                    // добавить к блюду ВСЕ ингредиенты
                    addIngredients(db, dish);
                    // добавить к блюду ДВА гарнира
                    DishGarnish garn;
                    garn = new DishGarnish() { RowGUID = Guid.NewGuid(), Price = 24, DishGUID = dish.RowGUID, RowPosition = 1 };
                    LangStringLib.SetValues(db, garn.RowGUID, 1, "Стеклянная лапша", "Cкляна локшина", "Glass noodles");
                    db.DishGarnish.Add(garn);

                    garn = new DishGarnish() { RowGUID = Guid.NewGuid(), Price = 19, DishGUID = dish.RowGUID, RowPosition = 2 };
                    LangStringLib.SetValues(db, garn.RowGUID, 1, "Рис басмати", "Рис басматі", "Basmati rice");
                    db.DishGarnish.Add(garn);
                    db.SaveChanges();

                    // наименование
                    LangStringLib.SetValues(db, dish.RowGUID, 1, namesList[0], namesList[1], namesList[2]);
                    // описание
                    LangStringLib.SetValues(db, dish.RowGUID, 2, descrList[0], descrList[1], descrList[2]);
                }
            }

            // блюда для третьего пункта меню (Салаты)
            namesList = new string[]{"Салат с крабовыми палочками «Красти»","Салат с крабовими паличками «Крастi»","Salad with crab \"Krusty\" sticks" };
            descrList = new string[] { "Всем знакомый и очень любимый, простой салат с крабовыми палочками, кукурузой и рисом для праздников и неожиданных гостей", "Всім знайомий і дуже коханий, простий салат з крабовими паличками, кукурудзою і рисом для свят і несподіваних гостей", "All the familiar and much loved, a simple salad with crab sticks, corn and rice for holidays and unexpected guests" };
            //addDishes(3, 4, l1, l2, 1, 250, 50, @"e:\Чернов\NoodleD\images\20161117-salat-s-krabovymi-palochkami-krasti-08.jpg");
            menuItemId = 3; cnt = 4; unitCount = 250; dishPrice = 50;
            imagePath = @"e:\Чернов\NoodleD\images\20161117-salat-s-krabovymi-palochkami-krasti-08.jpg";
            using (NoodleDContext db = new NoodleDContext())
            {
                m1 = db.MenuFolder.First(m => m.Id == menuItemId);
                for (int i = 0; i < cnt; i++)
                {
                    Dish dish = new Dish() { MenuFolderGUID = m1.RowGUID, RowGUID = Guid.NewGuid(),
                        UnitCount = unitCount, UnitGUID = dUnitGuid1, Price = dishPrice, RowPosition = i };
                    dish.Image = AppLib.getImageFromFilePath(imagePath);
                    db.Dish.Add(dish);
                    db.SaveChanges();

                    // добавить к блюду ВСЕ ингредиенты
                    addIngredients(db, dish);
                    // добавить к блюду ОДИН гарнир
                    DishGarnish garn;
                    garn = new DishGarnish() { RowGUID = Guid.NewGuid(), Price = 24, DishGUID = dish.RowGUID, RowPosition = 1 };
                    LangStringLib.SetValues(db, garn.RowGUID, 1, "Стеклянная лапша", "Cкляна локшина", "Glass noodles");
                    db.DishGarnish.Add(garn);
                    db.SaveChanges();

                    // наименование
                    LangStringLib.SetValues(db, dish.RowGUID, 1, namesList[0], namesList[1], namesList[2]);
                    // описание
                    LangStringLib.SetValues(db, dish.RowGUID, 2, descrList[0], descrList[1], descrList[2]);

                }
            }
            // салаты БЕЗ ингредиентов
            namesList = new string[] { "Салат «Старый год»", "Салат «Старий рiк»", "Salad \"Old Year\"" };
            descrList = new string[] { "С простым рецептом салата «Старый год» не придется долго возиться. Этот аппетитный салатик порадует ваших гостей своим вкусом, а вас простотой приготовления", "З простим рецептом салату «Старий рік» не доведеться довго возитися. Цей апетитний салатик порадує ваших гостей своїм смаком, а вас простотою приготування", "With a simple recipe salad \"old year\" will not have to mess around for a long time. This delicious salad will delight your guests with your taste and your cooking easy" };
            //            addDishes(3, 2, l1, l2, 1, 300, 60, @"e:\Чернов\NoodleD\images\20141222-salat-staryy-god-05.jpg");
            menuItemId = 3; cnt = 2; unitCount = 300; dishPrice = 60;
            imagePath = @"e:\Чернов\NoodleD\images\20141222-salat-staryy-god-05.jpg";
            using (NoodleDContext db = new NoodleDContext())
            {
                m1 = db.MenuFolder.First(m => m.Id == menuItemId);
                for (int i = 0; i < cnt; i++)
                {
                    Dish dish = new Dish() { MenuFolderGUID = m1.RowGUID, RowGUID = Guid.NewGuid(),
                        UnitCount = unitCount, UnitGUID = dUnitGuid1, Price = dishPrice, RowPosition = i };
                    dish.Image = AppLib.getImageFromFilePath(imagePath);
                    db.Dish.Add(dish);
                    db.SaveChanges();

                    // наименование
                    LangStringLib.SetValues(db, dish.RowGUID, 1, namesList[0], namesList[1], namesList[2]);
                    // описание
                    LangStringLib.SetValues(db, dish.RowGUID, 2, descrList[0], descrList[1], descrList[2]);
                }
            }

            // блюда для четвертого пункта меню (Супы)
            namesList = new string[] { "Суп-лапша с курицей «По-домашнему»", "Суп-локшина з куркою «По-домашньому»", "Noodle soup with chicken \"Home-style\"" };
            descrList = new string[] { "Вкусный куриный суп с домашней лапшой наполнит ваш дом ароматами тепла и уюта", "Смачний курячий суп з домашньою локшиною наповнить ваш будинок ароматами тепла і затишку", "Tasty chicken soup with homemade noodles fill your home with warmth and comfort flavors" };
            //            addDishes(4, 5, l1, l2, 2, 300, 44, @"e:\Чернов\NoodleD\images\sup-lapsha-s-kuricej-po-domashnemu.jpg");
            menuItemId = 4; cnt = 5; unitCount = 300; dishPrice = 44;
            imagePath = @"e:\Чернов\NoodleD\images\sup-lapsha-s-kuricej-po-domashnemu.jpg";
            using (NoodleDContext db = new NoodleDContext())
            {
                m1 = db.MenuFolder.First(m => m.Id == menuItemId);
                for (int i = 0; i < cnt; i++)
                {
                    Dish dish = new Dish() { MenuFolderGUID = m1.RowGUID, RowGUID = Guid.NewGuid(),
                        UnitCount = unitCount, UnitGUID = dUnitGuid2, Price = dishPrice, RowPosition= i };
                    dish.Image = AppLib.getImageFromFilePath(imagePath);
                    db.Dish.Add(dish);
                    db.SaveChanges();

                    // наименование
                    LangStringLib.SetValues(db, dish.RowGUID, 1, namesList[0], namesList[1], namesList[2]);
                    // описание
                    LangStringLib.SetValues(db, dish.RowGUID, 2, descrList[0], descrList[1], descrList[2]);
                }
            }

            // блюда для пятого пункта меню (Десерты)
            namesList = new string[] { "Запеченные яблоки в духовке", "Запечені яблука в духовці", "Baked apples in the oven" };
            descrList = new string[] { "Запеченные яблоки в духовке, это хит каждой осени и самый полезный десерт для всех возрастов. А приготовить их сможет каждая хозяйка", "Запечені яблука в духовці, це хіт кожної осені та найкорисніший десерт для будь-якого віку. А приготувати їх зможе кожна господиня", "Baked apples in the oven, it's a hit every autumn and most useful dessert for all ages. And they will be able to cook every woman" };
            //            addDishes(5, 3, l1, l2, 1, 400, 30, @"e:\Чернов\NoodleD\images\20160920-zapechennye-yabloki-v-duhovke-05.jpg");
            menuItemId = 5; cnt = 3; unitCount = 400; dishPrice = 30;
            imagePath = @"e:\Чернов\NoodleD\images\20160920-zapechennye-yabloki-v-duhovke-05.jpg";
            using (NoodleDContext db = new NoodleDContext())
            {
                m1 = db.MenuFolder.First(m => m.Id == menuItemId);
                for (int i = 0; i < cnt; i++)
                {
                    Dish dish = new Dish() { MenuFolderGUID = m1.RowGUID, RowGUID = Guid.NewGuid(),
                        UnitCount = unitCount, UnitGUID = dUnitGuid1, Price = dishPrice, RowPosition=i };
                    dish.Image = AppLib.getImageFromFilePath(imagePath);
                    db.Dish.Add(dish);
                    db.SaveChanges();

                    // наименование
                    LangStringLib.SetValues(db, dish.RowGUID, 1, namesList[0], namesList[1], namesList[2]);
                    // описание
                    LangStringLib.SetValues(db, dish.RowGUID, 2, descrList[0], descrList[1], descrList[2]);
                }
            }

            // блюда для шестого пункта меню (Напитки)
            namesList = new string[] { "Бананово-шоколадний коктейль", "Бананово-шоколадний коктейль", "Banana-chocolate cocktail" };
            //            addDishes(6, 4, l1, l2, 2, 350, 30, @"e:\Чернов\NoodleD\images\rphoto1340786322.jpg");
            menuItemId = 6; cnt = 4; unitCount = 350; dishPrice = 30;
            imagePath = @"e:\Чернов\NoodleD\images\rphoto1340786322.jpg";
            using (NoodleDContext db = new NoodleDContext())
            {
                m1 = db.MenuFolder.First(m => m.Id == menuItemId);
                for (int i = 0; i < cnt; i++)
                {
                    Dish dish = new Dish() { MenuFolderGUID = m1.RowGUID, RowGUID = Guid.NewGuid(),
                        UnitCount = unitCount, UnitGUID = dUnitGuid2, Price = dishPrice, RowPosition= i };
                    dish.Image = AppLib.getImageFromFilePath(imagePath);
                    db.Dish.Add(dish);
                    db.SaveChanges();

                    // наименование
                    LangStringLib.SetValues(db, dish.RowGUID, 1, namesList[0], namesList[1], namesList[2]);
                    // описание
                    LangStringLib.SetValues(db, dish.RowGUID, 2, descrList[0], descrList[1], descrList[2]);
                }
            }
        }

        private static void addIngredients(NoodleDContext db, Dish dish)
        {
            // добавить ингредиенты
            DishIngredient ingr;
            ingr = new DishIngredient() { RowGUID = Guid.NewGuid(), Price = 19, DishGUID = dish.RowGUID, RowPosition=1 };
            LangStringLib.SetValues(db, ingr.RowGUID, 1, "Двойной соус терияки", "Подвійний соус теріякі", "Double teriyaki sauce");
            db.DishIngredient.Add(ingr);

            ingr = new DishIngredient() { RowGUID = Guid.NewGuid(), Price = 9, DishGUID = dish.RowGUID, RowPosition = 2 };
            LangStringLib.SetValues(db, ingr.RowGUID, 1, "Двойная фасоль стручковая", "Подвійна квасоля стручкова", "Double runner beans");
            db.DishIngredient.Add(ingr);

            ingr = new DishIngredient() { RowGUID = Guid.NewGuid(), Price = 23, DishGUID = dish.RowGUID, RowPosition = 3 };
            LangStringLib.SetValues(db, ingr.RowGUID, 1, "Двойная курица", "Подвійна курка", "Double chicken");
            db.DishIngredient.Add(ingr);

            ingr = new DishIngredient() { RowGUID = Guid.NewGuid(), Price = 13, DishGUID = dish.RowGUID, RowPosition = 4 };
            LangStringLib.SetValues(db, ingr.RowGUID, 1, "Двойной деревестный гриб", "Подвійний дерев'янистий гриб", "Double tree fungus");
            db.DishIngredient.Add(ingr);

            ingr = new DishIngredient() { RowGUID = Guid.NewGuid(), Price = 18, DishGUID = dish.RowGUID, RowPosition = 5 };
            LangStringLib.SetValues(db, ingr.RowGUID, 1, "Двойные грибы шампиньоны", "Подвійні гриби печериці", "Double champignons");
            db.DishIngredient.Add(ingr);
        }

        private static void addGarnishes(NoodleDContext db, Dish dish)
        {
            // добавить гарниры
            DishGarnish garn;
            garn = new DishGarnish() { RowGUID = Guid.NewGuid(), Price = 24, DishGUID = dish.RowGUID, RowPosition = 1 };
            LangStringLib.SetValues(db, garn.RowGUID, 1, "Стеклянная лапша", "Cкляна локшина", "Glass noodles");
            db.DishGarnish.Add(garn);

            garn = new DishGarnish() { RowGUID = Guid.NewGuid(), Price = 19, DishGUID = dish.RowGUID, RowPosition = 2 };
            LangStringLib.SetValues(db, garn.RowGUID, 1, "Рис басмати", "Рис басматі", "Basmati rice");
            db.DishGarnish.Add(garn);

            garn = new DishGarnish() { RowGUID = Guid.NewGuid(), Price = 34, DishGUID = dish.RowGUID, RowPosition = 3 };
            LangStringLib.SetValues(db, garn.RowGUID, 1, "Яичная лапша", "Яєчна локшина", "Egg noodles");
            db.DishGarnish.Add(garn);
        }


        public static void setRecommends()
        {
            // добавить ко ВСЕМ блюдам по три случайные рекомендации
            using (NoodleDContext db = new NoodleDContext())
            {
                List<Dish> dList = db.Dish.ToList();
                Random rnd = new Random(); int cnt = db.Dish.Count(), curRnd;
                foreach (Dish dish in db.Dish)
                {
                    curRnd = rnd.Next(1, cnt);
                    Dish dRec1 = dList.ElementAt(curRnd);

                    curRnd = rnd.Next(1, cnt);
                    Dish dRec2 = dList.ElementAt(curRnd);

                    curRnd = rnd.Next(1, cnt);
                    Dish dRec3 = dList.ElementAt(curRnd);

                    db.DishRecommends.Add(new DishRecommends() { DishGUID = dish.RowGUID, RecommendGUID = dRec1.RowGUID, RowPosition = 1 });
                    db.DishRecommends.Add(new DishRecommends() { DishGUID = dish.RowGUID, RecommendGUID = dRec2.RowGUID, RowPosition = 2 });
                    db.DishRecommends.Add(new DishRecommends() { DishGUID = dish.RowGUID, RecommendGUID = dRec3.RowGUID, RowPosition = 3 });
                }
                db.SaveChanges();
            }
        }

        private static DishMark getDishMark(NoodleDContext db, int id)
        {
            return db.DishMark.First(dm => dm.Id == id);
        }

    }
}
