using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using UserActionLog;
using AppActionNS;
using AppModel;
using System.Threading;
using System.Windows.Controls;
using SplashScreenLib;
using IntegraLib;
using WpfClient.Views;
using System.Reflection;
using IntegraWPFLib;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, Microsoft.Shell.ISingleInstanceApp
    {
        private const string Unique = "My_Unique_Application_String";
        
        // специальные логгеры событий WPF
        public static UserActionIdle IdleHandler = null;

        public static string DeviceId;
        public static string OrderNumber;
        public static string PromocodeNumber;

        [STAThread]
        public static void Main()
        {
            App app = new App();

            if (Microsoft.Shell.SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                string cfgValue;
                // установить текущий каталог на папку с приложением
                setAppDirectory();

                // splash-screen
                Splasher.Splash = new Views.SplashScreen();
                Splasher.ShowSplash();

                MessageListener.Instance.ReceiveMessage("Инициализация журнала событий...");
                cfgValue = AppLib.InitAppLoggers();
                if (cfgValue != null)
                {
                    appExit(1, "Ошибка инициализации журнала приложения: " + cfgValue);
                }

                AppLib.WriteLogInfoMessage("************  Start NoodleD_Client (WPF) *************");
                // объем доступной памяти
                MessageListener.Instance.ReceiveMessage("Check free RAM value...");
                int freeMemory = AppLib.getAvailableRAM();
                AppLib.WriteLogInfoMessage("Available RAM: " + freeMemory.ToString() + " MB");
                if (freeMemory < 300)
                {
                    AppLib.WriteLogErrorMessage("Terminate program by low memory.");
                    AppLib.WriteLogInfoMessage("************  End application  ************");
                    appExit(2, "This computer has too low available memory.\r\nYou need at least 300 MB free memory.");
                }

                // таймаут запуска приложения
                cfgValue = CfgFileHelper.GetAppSetting("StartTimeout");
                int startTimeout = 0;
                if (cfgValue != null) startTimeout = cfgValue.ToInt();
                if (startTimeout != 0)
                {
                    for (int i = startTimeout; i > 0; i--)
                    {
                        MessageListener.Instance.ReceiveMessage($"Таймаут запуска приложения - {i} секунд.");
                        System.Threading.Thread.Sleep(1000);
                    }
                }

                #region информация о сборках
                MessageListener.Instance.ReceiveMessage("Получаю информацию о сборках и настройках...");
                ITSAssemblyInfo asmInfo = new ITSAssemblyInfo(AppEnvironment.GetAppAssemblyName());
                AppLib.WriteLogInfoMessage($" - файл: {asmInfo.FullFileName}, version {asmInfo.Version}, last write date '{asmInfo.DateLastOpened.ToString()}'");
                asmInfo = new ITSAssemblyInfo("IntegraLib");
                AppLib.WriteLogInfoMessage($" - файл: {asmInfo.FullFileName}, version {asmInfo.Version}, last write date '{asmInfo.DateLastOpened.ToString()}'");
                asmInfo = new ITSAssemblyInfo("IntegraWPFLib");
                AppLib.WriteLogInfoMessage($" - файл: {asmInfo.FullFileName}, version {asmInfo.Version}, last write date '{asmInfo.DateLastOpened.ToString()}'");

                AppLib.WriteLogInfoMessage("Системное окружение: " + AppEnvironment.GetEnvironmentString());

                // номер устройства - не число!
                if (AppLib.GetAppSetting("ssdID").IsNumber() == false)
                {
                    AppLib.WriteLogErrorMessage("** Номер устройства - НЕ ЧИСЛО !! **");
                    AppLib.WriteLogInfoMessage("************  End application  ************");
                    appExit(4, "Номер устройства - НЕ ЧИСЛО!!");
                }

                // основная информация о софт-окружении
                AppLib.WriteLogTraceMessage(string.Format("Настройки: Id устройства-{0}, папка изображений-{1}, таймер бездействия-{2} sec, диапазон номеров чеков от {3} до {4}, принтер пречека-{5}, отладка: IsLogUserAction-{6}, IsWriteTraceMessages-{7}, IsWriteWindowEvents-{8}",
                    AppLib.GetAppSetting("ssdID"), AppLib.GetAppSetting("ImagesPath"), AppLib.GetAppSetting("UserIdleTime"),
                    AppLib.GetAppSetting("RandomOrderNumFrom"), AppLib.GetAppSetting("RandomOrderNumTo"),
                    AppLib.GetAppSetting("PrinterName"), AppLib.GetAppSetting("IsLogUserAction"), AppLib.GetAppSetting("IsWriteTraceMessages"), AppLib.GetAppSetting("IsWriteWindowEvents")));

                //******  НАСТРОЙКИ  ******
                // определенные в app.xaml
                app.InitializeComponent();
                // определенные в config-файле
                getSettingsFromConfigFile();     
                // вычислить размеры, хранимые в свойствах приложения
                calculateAppSizes();
                // прочие глобальные настройки
                AppLib.SetAppGlobalValue("promoCode", null);
                //TestData.mainProc();
                #endregion

                // проверка соединения с бд
                MessageListener.Instance.ReceiveMessage("Проверяю доступность к базе данных...");
                AppLib.WriteLogTraceMessage("Проверка доступа к базе данных...");
                AppLib.WriteLogTraceMessage(" - строка подключения: " + getDbConnectionString());
                string errorMessage;
                while (AppLib.CheckDBConnection(typeof(NoodleDContext), out errorMessage) == false)
                {
                    AppLib.WriteLogTraceMessage(" - result: " + errorMessage);
                    // задержка на 10 сек
                    for (int i = 10; i > 0; i--)
                    {
                        MessageListener.Instance.ReceiveMessage($"Проверка доступа к БД завершилась ошибкой!! (след.проверка через {i} сек)");
                        Thread.Sleep(1000);
                    }
                    MessageListener.Instance.ReceiveMessage("Проверяю доступность к базе данных...");
                    Thread.Sleep(500);
                }
                AppLib.WriteLogTraceMessage(" - result: Ok");

                // настройки, определенные в ms sql
                readSettingFromDB();

                // данные, хранящиеся в БД
                readAppDataFromDB();

                // ожидашка
                int idleSec = (int)AppLib.GetAppGlobalValue("UserIdleTime");
                if (idleSec > 0)
                {
                    IdleHandler = new UserActionIdle();
                    IdleHandler.IdleSeconds = idleSec;
                    IdleHandler.IdleElapseEvent += IdleHandler_IdleElapseEvent;
                    IdleHandler.SetPause();
                }

                // главное окно приложения
                MessageListener.Instance.ReceiveMessage("Запуск основного окна...");
                Thread.Sleep(500);

                MainWindow mainWindow = new MainWindow();
                try
                {
                    app.Run(mainWindow);
                }
                catch (Exception ex)
                {
                    AppLib.WriteLogErrorMessage(ex.ToString());
                    MessageBox.Show(ex.Message, "Error Application", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                }

                AppLib.WriteLogInfoMessage("************  End application  ************");

                // подчистить память
                if (IdleHandler != null) IdleHandler.Dispose();
                AppLib.AppActionLogger.Close();

                // Allow single instance code to perform cleanup operations
                Microsoft.Shell.SingleInstance<App>.Cleanup();
            }
        }


        private static void appExit(int exitCode, string errMsg)
        {
            if (Splasher.Splash != null) Splasher.CloseSplash();

            if ((exitCode != 0) && (errMsg.IsNull() == false))
            {
                MessageBox.Show(errMsg, "Аварийное завершение программы", MessageBoxButton.OK, MessageBoxImage.Stop);
            }

            Environment.Exit(exitCode);
        }


        #region таймер бездействия

        public static void IdleTimerStart(Window targetWindow)
        {
            if (App.IdleHandler != null) App.IdleHandler.CurrentWindow = targetWindow;
        }
        public static void IdleTimerStop()
        {
            if (App.IdleHandler != null) App.IdleHandler.CurrentWindow = null;
        }

        private static void IdleHandler_IdleElapseEvent(System.Timers.ElapsedEventArgs obj)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (idleAction() == false) App.IdleHandler.SetPause();
            });
        }

        // окно Ожидашки
        private static bool idleAction()
        {
            // условия, при которых таймер бездействия ставится на паузу
            if (AppLib.IsOpenWindow("MsgBoxExt", "idleWin"))
            {
                AppLib.WriteLogTraceMessage("Таймер ожидания: ожидашка уже открыта");
                return false;   // само окно бездействия
            }

            // продолжаем, т.е. показываем окно бездействия, если открыты некоторые окна или есть блюда в корзине
            AppModel.OrderItem order = (AppModel.OrderItem)AppLib.GetAppGlobalValue("currentOrder");
            bool isContinue = AppLib.IsOpenWindow("Cart") 
                || AppLib.IsOpenWindow("DishPopup") || AppLib.IsOpenWindow("Promocode") 
                || ((order.Dishes != null) && (order.Dishes.Count > 0));
            if (isContinue == false)
            {
                AppLib.WriteLogTraceMessage("Таймер ожидания: не выполнены условия показа ожидашки");
                return false; 
            }

            MsgBoxExt mBox = new MsgBoxExt()
            {
                Name = "idleWin",
                ShowActivated = true,
                BigButtons = true, IsShowTitle = false, IsMessageCentered = true, IsRoundCorner = true,
                MessageFontSize = (double)AppLib.GetAppGlobalValue("appFontSize1"),
                ButtonFontSize = (double)AppLib.GetAppGlobalValue((AppLib.IsAppVerticalLayout) ? "appFontSize2" : "appFontSize1"),

                MsgBoxButton = MessageBoxButton.YesNo,

                ButtonForeground = Brushes.White,
                ButtonBackground = (Brush)AppLib.GetAppGlobalValue("appBackgroundColor"),
                ButtonBackgroundOver = (Brush)AppLib.GetAppGlobalValue("appBackgroundColor"),
            };
            mBox.CloseByButtonPress = true;
            var v = AppLib.GetAppGlobalValue("autoUIReset");
            double dInterval = (v == null) ? 10000 : (int)v * 1000;   // in msec
            mBox.SetAutoCloseTimer(dInterval, 500, 
                (sender, e) => 
                {
                    double remainSec = Math.Round(e.RemainMilliSeconds / 1000d, 1);
                    //mBox.MessageText = AppLib.GetLangTextFromAppProp("areYouHereQuestion") + "\nДо закрытия окна осталось " + remainSec.ToString() + " sec";
                    mBox.btn2Text.Text = AppLib.GetLangTextFromAppProp("dialogBoxNoText") + " (" + remainSec.ToString("0.0") + ")";
                });
            mBox.MessageText = AppLib.GetLangTextFromAppProp("areYouHereQuestion");

            // надписи на кнопках Да/Нет согласно выбранному языку
            string sYes = AppLib.GetLangTextFromAppProp("dialogBoxYesText");
            string sNo = AppLib.GetLangTextFromAppProp("dialogBoxNoText");
            mBox.ButtonsText = string.Format(";;{0};{1}", sYes, sNo);

            AppLib.WriteLogTraceMessage("Ожидашка открывается...");
            AppLib.WriteAppAction(mBox.Name, AppActionsEnum.IdleWindowOpen);

            MessageBoxResult result = mBox.ShowDialog();

            AppLib.WriteLogTraceMessage("Ожидашка закрывается...");
            AppLib.WriteAppAction(mBox.Name, AppActionsEnum.IdleWindowClose, result.ToString());

            // reset UI
            bool retVal = false;
            if (result == MessageBoxResult.Yes)
            {
                // чтобы не срабатывали обработчики нижележащих контролов
                AppLib.IsEventsEnable = false;
                retVal = true;
            }
            else
            {
                AppLib.ReStartApp(true, true, false);
            }
            return retVal;
        }

        #endregion

        #region ISingleInstanceApp Members

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            // handle command line arguments of second instance
            // …
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            // проверка наличия экземпляра в памяти
            string appProcName = assembly.FullName.Split(',')[0];

            MessageBox.Show("Application " + appProcName + " is already running.");

            return true;
        }

        #endregion

        #region работа с БД
        private static string getDbConnectionString()
        {
            string retVal = null;
            using (NoodleDContext dbContext = new NoodleDContext())
            {
                retVal = dbContext.Database.Connection.ConnectionString;
            }
            return retVal;
        }

        private static void checkDBConnection()
        {
            string logMsg = "Проверяю соединение с источником данных...";
            AppLib.WriteLogTraceMessage(logMsg);

            Configuration con = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            string connectionName = null;
            string connectionString = null;
            foreach (ConnectionStringSettings item in ConfigurationManager.ConnectionStrings)
            {
                if (item.ProviderName == "System.Data.EntityClient")
                {
                    connectionString = item.ConnectionString;
                    int i = connectionString.IndexOf("connection string=", 0);
                    connectionString = connectionString.Substring(i + 19);
                    connectionString = connectionString.Substring(0, connectionString.Length - 1);
                    connectionName = item.Name; break;
                }
            }
            if (connectionName == null)
            {
                throw new Exception("Cannot find EntityClient connection string in application config file.");
            }

            AppLib.WriteLogTraceMessage(" - connection string: " + connectionString);
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
            else
            {
                throw new Exception("Cannot open EF connection " + connectionName + " by its connection string: " + connectionString);
            }
            AppLib.WriteLogTraceMessage(logMsg + " READY");
        }
        #endregion

        #region настройка приложения

        private static void getSettingsFromConfigFile()
        {
            AppLib.WriteLogTraceMessage("Читаю настройки из файла *.config ...");

            // фоновое изображение
            saveAppSettingToProps("BackgroundImageHorizontal");
            saveAppSettingToProps("BackgroundImageVertical");
            saveAppSettingToProps("BackgroundImageBrightness", typeof(double), "0.2");

            // идентификатор устройства самообслуживания
            saveAppSettingToProps("ssdID", null);
            App.DeviceId = (string)AppLib.GetAppGlobalValue("ssdID");

            // путь к папке с изображениями
            saveAppSettingToProps("ImagesPath");

            // символ денежной единицы
            saveAppSettingToProps("CurrencyChar", null);

            // печать чека
            // ширина в пикселях (1"=96px => 1px = 0.26mm)
            saveAppSettingToProps("BillPageWidht", typeof(int), "300");
            // размер шрифта позиций заказа
            saveAppSettingToProps("BillLineFontSize", typeof(int), "12");
            // отступ слева строк позиций заказа, в пикселях (1px = 0.26mm)
            saveAppSettingToProps("BillLineLeftMargin", typeof(int), "0");
            // отступ сверху строки блюда, в пикселях (1px = 0.26mm)
            saveAppSettingToProps("BillLineTopMargin", typeof(int), "10");
            // отступ сверху строки ингредиента, в пикселях (1px = 0.26mm)
            saveAppSettingToProps("BillLineIngrTopMargin", typeof(int), "0");
            // отступ сверху строки цены, в пикселях (1px = 0.26mm)
            saveAppSettingToProps("BillLinePriceTopMargin", typeof(int), "0");

            // большие кнопки прокрутки панели блюд
            saveAppSettingToProps("dishesPanelScrollButtonSize", typeof(double));
            saveAppSettingToProps("dishesPanelScrollButtonHorizontalAlignment");

            // размер шрифта заголовка панели блюда
            saveAppSettingToProps("dishPanelHeaderFontSize", typeof(int));
            saveAppSettingToProps("dishPanelUnitCountFontSize", typeof(int));
            saveAppSettingToProps("dishPanelDescriptionFontSize", typeof(int));
            saveAppSettingToProps("dishPanelAddButtoFontSize", typeof(int));
            saveAppSettingToProps("dishPanelFontSize", typeof(int));
            saveAppSettingToProps("dishPanelGarnishBrightness");

            saveAppSettingToPropTypeBool("IsPrintBarCode");
            saveAppSettingToPropTypeBool("IsIncludeBarCodeLabel");
            saveAppSettingToPropTypeBool("isAnimatedSelectVoki");

            saveAppSettingToPropTypeBool("IsLogUserAction");
            saveAppSettingToPropTypeBool("IsWriteTraceMessages");

            // добавить некоторые постоянные тексты (заголовки, надписи на кнопках)
            parseAndSetAllLangString("dialogBoxYesText");
            parseAndSetAllLangString("dialogBoxNoText");
            parseAndSetAllLangString("wordIngredients");
            parseAndSetAllLangString("InputNumberWinTitle");
            parseAndSetAllLangString("cartDelIngrTitle");
            parseAndSetAllLangString("cartDelIngrQuestion");
            parseAndSetAllLangString("cartDelDishTitle");
            parseAndSetAllLangString("cartDelDishQuestion");
            // сообщения печати
            parseAndSetAllLangString("printOrderTitle");
            parseAndSetAllLangString("saveOrderErrorMessage");
            parseAndSetAllLangString("userErrMsgSuffix");
            parseAndSetAllLangString("afterPrintingErrMsg");
            parseAndSetAllLangString("printConfigError");
            parseAndSetAllLangString("printerStatusMsg");
            // TakeOrder window
            parseAndSetAllLangString("takeOrderOut");
            parseAndSetAllLangString("wordOr");
            parseAndSetAllLangString("takeOrderIn");

            // AreYouHere window
            saveAppSettingToProps("UserIdleTime", typeof(int));        // время бездействия из config-файла, в сек
            parseAndSetAllLangString("areYouHereTitle");
            parseAndSetAllLangString("areYouHereQuestion");
            // время в секундах, через которое произойдет возврат приложения в исходное состояние, если пользователь не нажал Да
            saveAppSettingToProps("autoUIReset", typeof(int));

            parseAndSetAllLangString("CurrencyName");
            parseAndSetAllLangString("withGarnish");

            AppLib.WriteLogTraceMessage("Читаю настройки из файла *.config ... READY");
        }

        // сохранить настройки приложения из config-файла в свойствах приложения
        public static bool saveAppSettingToProps(string settingName, Type settingType = null, string defaultConfValue = null)
        {
            string settingValue = CfgFileHelper.GetAppSetting(settingName);
            if (settingValue == null)
            {
                if (defaultConfValue == null)
                    return false;
                else
                    settingValue = defaultConfValue;
            }

            if (settingType == null)
            {
                AppLib.SetAppGlobalValue(settingName, settingValue);   // по умолчанию сохраняется как строка
            }
            else
            {
                object objValue = getObjValueFromString(settingValue, settingType);
                if (objValue != null) AppLib.SetAppGlobalValue(settingName, objValue);
            }
            return true;
        }

        private static object getObjValueFromString(string valString, Type valType)
        {
            object retVal = null;
            if (valType.Equals(typeof(double)))
            {
                retVal = Convert.ToDouble(valString, System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (valType.Equals(typeof(float)))
            {
                retVal = Convert.ToSingle(valString, System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (valType.Equals(typeof(decimal)))
            {
                retVal = Convert.ToDecimal(valString, System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                MethodInfo mi = valType.GetMethods().FirstOrDefault(m => m.Name == "Parse");
                // если у типа есть метод Parse
                if (mi != null)  // то распарсить значение
                {
                    object classInstance = Activator.CreateInstance(valType);
                    retVal = mi.Invoke(classInstance, new object[] { valString });
                }
                else
                    retVal = valString;   // по умолчанию сохраняется как строка
            }
            return retVal;
        }

        // сохранить настройку приложения из config-файла в bool-свойство приложения
        private static void saveAppSettingToPropTypeBool(string settingName)
        {
            string settingValue = AppLib.GetAppSetting(settingName);
            if (settingValue == null) return;

            // если значение истина, true или 1, то сохранить в свойствах приложения True, иначе False
            if (settingValue.ToBool() == true)
                AppLib.SetAppGlobalValue(settingName, true);
            else
                AppLib.SetAppGlobalValue(settingName, false);
        }

        private static void parseAndSetAllLangString(string resKey)
        {
            string resValue = AppLib.GetAppSetting(resKey);
            if (string.IsNullOrEmpty(resValue) == true) return;

            string[] aStr = resValue.Split('|');
            if (aStr.Length != 3) return;

            Dictionary<string, string> d = new Dictionary<string, string>();
            d.Add("ru", aStr[0]); d.Add("ua", aStr[1]); d.Add("en", aStr[2]);
            AppLib.SetAppGlobalValue(resKey, d);
        }


        private static void setAppDirectory()
        {
            string curDir = System.IO.Directory.GetCurrentDirectory();
            if (curDir.Last() != '\\') curDir += "\\";
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            if (curDir.Equals(appDir, StringComparison.OrdinalIgnoreCase) == false)
            {
                AppLib.WriteLogTraceMessage($"Текущий каталог '{curDir}' НЕ установлен в папку приложения '{appDir}'. Изменяю текущий каталог...");
                try
                {
                    System.IO.Directory.SetCurrentDirectory(appDir);
                    AppLib.WriteLogTraceMessage("Текущий каталог установлен в папку приложения успешно");
                }
                catch (Exception ex)
                {
                    AppLib.WriteLogErrorMessage("Ошибка изменения текущего каталога: " + ex.Message);
                    appExit(2, "Error: " + ex.Message);
                }
            }
        }

        public static void readSettingFromDB()
        {
            MessageListener.Instance.ReceiveMessage("Читаю настройки из БД...");
            AppLib.WriteLogTraceMessage("Получаю настройки приложения из таблицы Setting ...");

            using (NoodleDContext db = new NoodleDContext())
            {
                try
                {
                    List<Setting> setList = db.Setting.ToList();
                    List<StringValue> stringTable = db.StringValue.ToList();

                    foreach (Setting item in setList)
                    {
                        //                        AppLib.WriteLogTraceMessage("- параметр "+ item.UniqName + "...");

                        AppLib.SetAppGlobalValue(item.UniqName, item.Value);
                        if (item.UniqName.EndsWith("Color") == true)  // преобразовать и переопределить цвета
                        {
                            convertAppColor(item.UniqName);
                            checkAppColor(item.UniqName);
                        }
                        if (item.Value == "StringValue")    // заменить в Application.Properties строку StringValue на словарь языковых строк
                        {
                            Dictionary<string, string> d = getLangTextDict(stringTable, item.RowGUID, 1);
                            AppLib.SetAppGlobalValue(item.UniqName, d);
                        }

                        //                        AppLib.WriteLogTraceMessage("- параметр " + item.UniqName + "... Ready");
                    }
                }
                catch (Exception ex)
                {
                    AppLib.WriteLogErrorMessage(string.Format("Fatal error: {0}\nSource: {1}\nStackTrace: {2}", ex.Message, ex.Source, ex.StackTrace));
                    appExit(5, "Ошибка доступа к данным: " + ex.Message + "\nПрограмма будет закрыта.");
                }
            }
            AppLib.WriteLogTraceMessage("Получаю настройки приложения из таблицы Setting ... READY");
        }

        // установка цвета ресурса приложения (Application.Resources) в цвет из свойств приложения (Application.Properties)
        private static void checkAppColor(string setName)
        {
            SolidColorBrush bRes = (SolidColorBrush)Application.Current.Resources[setName];
            if (bRes == null) return;

            SolidColorBrush bProp = (SolidColorBrush)AppLib.GetAppGlobalValue(setName);

            if (bRes.Color.Equals(bProp.Color) == false)  // если не равны
            {
                Application.Current.Resources[setName] = bProp;   // то переопределить ресурсную кисть
            }
        }
        private static Dictionary<string, string> getLangTextDict(List<StringValue> stringTable, Guid rowGuid, int fieldTypeId)
        {
            Dictionary<string, string> retVal = new Dictionary<string, string>();
            foreach (StringValue item in
                from val in stringTable where val.RowGUID == rowGuid && val.FieldType.Id == fieldTypeId select val)
            {
                if (retVal.Keys.Contains(item.Lang) == false) retVal.Add(item.Lang, item.Value);
            }
            return retVal;
        }


        public static void readAppDataFromDB()
        {
            MessageListener.Instance.ReceiveMessage("Получаю из MS SQL главное меню...");
            AppLib.WriteLogTraceMessage("Получаю из MS SQL главное меню...");

            MenuLib.MenuFolderHandler = (folderName) => MessageListener.Instance.ReceiveMessage($"Получаю блюда раздела '{folderName}'...");
            List<AppModel.MenuItem> newMenu = MenuLib.GetMenuMainFolders();
            if (newMenu == null)
            {
                AppLib.WriteLogErrorMessage("Fatal error: Ошибка создания Главного Меню. Меню не создано. Аварийное завершение приложения.");
                MessageBox.Show("Ошибка создания меню\nПрограмма будет закрыта.");
                throw new Exception("Ошибка создания меню");
            }

            // сохранить Главное Меню в свойствах приложения
            List<AppModel.MenuItem> mainMenu = (List<AppModel.MenuItem>)AppLib.GetAppGlobalValue("mainMenu");
            if (mainMenu != null) mainMenu.Clear();
            mainMenu = newMenu;

            AppLib.SetAppGlobalValue("mainMenu", mainMenu);

            AppLib.WriteLogTraceMessage("Получаю из MS SQL главное меню... - READY");
        }

        // преобразование строки цветов (R,G,B) в SolidColorBrush
        private static void convertAppColor(string setName)
        {
            var buf = AppLib.GetAppGlobalValue(setName);
            if ((buf is string) == false) return;

            // если цвет задан строкой
            string sBuf = (string)buf;
            string[] sRGB = sBuf.Split(',');
            byte r = 0, g = 0, b = 0;
            byte.TryParse(sRGB[0], out r);
            byte.TryParse(sRGB[1], out g);
            byte.TryParse(sRGB[2], out b);
            SolidColorBrush brush = new SolidColorBrush(new Color() { A = 255, R = r, G = g, B = b });

            AppLib.SetAppGlobalValue(setName, brush);
        }


        private static void createAppResources(Application app)
        {
            // ресурсы приложения
            //< !--цвета приложения по умолчанию-- >
            app.Resources.Add("appColorDarkPink", Color.FromArgb(255, 122, 34, 104)); // < !--COLOR1 - dark pink-- >
            app.Resources.Add("appColorYellow", Color.FromArgb(255, 255, 200, 62));  //< !--COLOR2 - dark yellow-- >
            app.Resources.Add("appColorWhite", Color.FromArgb(255, 255, 255, 255));        // < !--COLOR3 - white-- >
            app.Resources.Add("appColorDDarkPink", Color.FromArgb(255, 99, 29, 85));        // < !--COLOR4 - dark - dark pink-- >
            app.Resources.Add("appColorSelectButton", Color.FromArgb(255, 173, 32, 72)); //< !--COLOR5 - dark - dark pink-- >
            app.Resources.Add("appColorCartButtom", Color.FromArgb(255, 214, 244, 36));  //< !--COLOR6 - green - yellow-- >
            app.Resources.Add("appColorBackgroundGarnish", Color.FromArgb(255, 137, 137, 137));  //< !--COLOR7 - grey-- >

            // кисти
            app.Resources.Add("appBackgroundColor", new SolidColorBrush((Color)app.Resources["appColorDarkPink"]));
            app.Resources.Add("appNotSelectedItemColor", new SolidColorBrush((Color)app.Resources["appColorWhite"]));
            app.Resources.Add("appSelectedItemColor", new SolidColorBrush((Color)app.Resources["appColorYellow"]));
            app.Resources.Add("mainMenuSelectedItemColor", new SolidColorBrush((Color)app.Resources["appColorDDarkPink"]));
            app.Resources.Add("addButtonBackgroundTextColor", new SolidColorBrush((Color)app.Resources["appColorSelectButton"]));
            app.Resources.Add("addButtonBackgroundPriceColor", new SolidColorBrush(Color.FromArgb(255, 147, 29, 63)));
            app.Resources.Add("cartButtonBackgroundColor", new SolidColorBrush((Color)app.Resources["appColorCartButtom"]));
            app.Resources.Add("garnishBackgroundColor", new SolidColorBrush((Color)app.Resources["appColorSelectGarnish"]));
            app.Resources.Add("winShadowColor", new SolidColorBrush(Color.FromArgb(0x88, 0, 0, 0)));

            // конвертеры
            app.Resources.Add("isNullValueConverter", new IsNullValueConverter());
            app.Resources.Add("getAppSetValue", new GetAppSetValue());
            app.Resources.Add("langDictToText", new LangDictToTextConverter());
            app.Resources.Add("langDictToUpperText", new LangDictToTextConverter() { IsUpper = true });
            app.Resources.Add("multiplyParamConv", new MultiplyParamValueConverter());
            app.Resources.Add("getMinValue", new GetMinValue());
            app.Resources.Add("upperCaseConverter", new UpperCaseConverter());
            app.Resources.Add("isEmptyEnumerator", new IsEmptyEnumerator());
            app.Resources.Add("cornerRadiusLeft", new Views.CornerRadiusConverter() { Side = "Left" });
            app.Resources.Add("cornerRadiusRight", new Views.CornerRadiusConverter() { Side = "Right" });
            app.Resources.Add("garnishLangTextConverter", new GarnishLangTextConverter());
            app.Resources.Add("garnishPriceConverter", new GarnishPriceConverter());
            app.Resources.Add("getMargin", new GetMargin());
            app.Resources.Add("converterChain", new ConverterChain());
            // стили
            Style centeredElement = new Style(typeof(FrameworkElement));
            centeredElement.Setters.Add(new Setter(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center));
            centeredElement.Setters.Add(new Setter(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center));
            app.Resources.Add("centeredElement", centeredElement);
            // прочие ресурсы
            app.Resources.Add("priceFormatString", "{0:#0} ₴");

        }

        private static void calculateAppSizes()
        {
            double dVar;

            double screenWidth, screenHeight;
            screenWidth = (double)AppLib.GetAppGlobalValue("screenWidth");
            screenHeight = (double)AppLib.GetAppGlobalValue("screenHeight");
            AppLib.WriteLogTraceMessage(string.Format("Монитор - {0} x {1}", screenWidth, screenHeight));

            double dishesPanelWidth;
            // вертикальная разметка: панель меню сверху
            if (AppLib.IsAppVerticalLayout)
            {
                // панель меню
                AppLib.SetAppGlobalValue("categoriesPanelWidth", screenWidth);
                AppLib.SetAppGlobalValue("categoriesPanelHeight", (screenHeight / 6d * 1.0d));
                // панель блюд
                dishesPanelWidth = screenWidth;
                AppLib.SetAppGlobalValue("dishesPanelHeight", (screenHeight / 6d * 5.0d));
            }
            // горизонтальная разметка: панель меню справа
            else
            {
                // панель меню
                AppLib.SetAppGlobalValue("categoriesPanelWidth", (screenWidth / 6d * 1d));
                AppLib.SetAppGlobalValue("categoriesPanelHeight", screenHeight);
                // панель блюд
                dishesPanelWidth = (screenWidth / 6d * 5d);
                AppLib.SetAppGlobalValue("dishesPanelHeight", screenHeight);
            }
            AppLib.SetAppGlobalValue("dishesPanelWidth", dishesPanelWidth);

            // кол-во колонок панелей блюд
            saveAppSettingToProps("dishesColumnsCount");
            int dColCount = AppLib.GetAppGlobalValue("dishesColumnsCount", 0).ToString().ToInt();
            if (dColCount == 0)
            {
                dColCount = 3;
                AppLib.SetAppGlobalValue("dishesColumnsCount", dColCount);
            }
            // ***************
            // панель блюда
            double dishPanelWidth = 0.95d * dishesPanelWidth / dColCount;
            AppLib.SetAppGlobalValue("dishPanelWidth", dishPanelWidth);
            double contentPanelWidth = 0.9d * dishPanelWidth;
            AppLib.SetAppGlobalValue("dishImageWidth", contentPanelWidth);
            AppLib.SetAppGlobalValue("contentPanelWidth", contentPanelWidth);
            double dishPanelLeftMargin = (dishesPanelWidth - dColCount * dishPanelWidth) / 2;
            AppLib.SetAppGlobalValue("dishPanelLeftMargin", dishPanelLeftMargin);
            // высота строки заголовка
            double dishPanelHeaderRowHeight = (AppLib.IsAppVerticalLayout ? 0.25d : 0.17d) * dishPanelWidth;
            if (dColCount == 2) dishPanelHeaderRowHeight *= 0.5;
            AppLib.SetAppGlobalValue("dishPanelHeaderRowHeight", dishPanelHeaderRowHeight);
            // высота строки изображения
            double dishPanelImageRowHeight = 0.7d * dishPanelWidth;
            AppLib.SetAppGlobalValue("dishPanelImageRowHeight", dishPanelImageRowHeight);
            AppLib.SetAppGlobalValue("dishImageHeight", dishPanelImageRowHeight);
            // высота строки гарниров
            double dishPanelGarnishesRowHeight = ((AppLib.IsAppVerticalLayout)?0.3d:0.2d) * dishPanelWidth;
            AppLib.SetAppGlobalValue("dishPanelGarnishesRowHeight", dishPanelGarnishesRowHeight);
            // высота строки кнопки добавления
            double dishPanelAddButtonRowHeight = (AppLib.IsAppVerticalLayout?0.25d:0.15d) * dishPanelWidth;
            AppLib.SetAppGlobalValue("dishPanelAddButtonRowHeight", dishPanelAddButtonRowHeight);
            AppLib.SetAppGlobalValue("dishPanelAddButtonTextSize", 0.3d * dishPanelAddButtonRowHeight);
            // расстояния между строками панели блюда
            double dishPanelRowMargin1 = 0.01d * dishPanelWidth;
            double dishPanelRowMargin2 = (AppLib.IsAppVerticalLayout?0.03d:0.02d) * dishPanelWidth;
            AppLib.SetAppGlobalValue("dishPanelRowMargin1", dishPanelRowMargin1);
            AppLib.SetAppGlobalValue("dishPanelRowMargin2", dishPanelRowMargin2);
            // размер кнопки описания блюда
            AppLib.SetAppGlobalValue("dishPanelDescrButtonSize", 0.085d * dishPanelWidth);
            // высота панелей
            double dishPanelHeight = Math.Floor(dishPanelHeaderRowHeight + dishPanelRowMargin1 + dishPanelImageRowHeight + dishPanelRowMargin2 + dishPanelAddButtonRowHeight);
            double dishPanelHeightWithGarnish = Math.Floor(dishPanelHeight + dishPanelGarnishesRowHeight + dishPanelRowMargin2);
            AppLib.SetAppGlobalValue("dishPanelHeight", dishPanelHeight);
            AppLib.SetAppGlobalValue("dishPanelHeightWithGarnish", dishPanelHeightWithGarnish);
            dVar = 1d;
            double contentPanelHeight = Math.Floor(dVar * dishPanelHeight);
            double contentPanelHeightWithGarnish = Math.Floor(dVar * dishPanelHeightWithGarnish);
            AppLib.SetAppGlobalValue("contentPanelHeight", contentPanelHeight);
            AppLib.SetAppGlobalValue("contentPanelHeightWithGarnish", contentPanelHeightWithGarnish);
            // ***********

            // углы закругления
            dVar = 0.005 * screenWidth;
            AppLib.SetAppGlobalValue("cornerRadiusButton", dVar);
            AppLib.SetAppGlobalValue("cornerRadiusGarnish", 0.5 * dVar);
            AppLib.SetAppGlobalValue("cornerRadiusDishPanel", 2 * dVar);

            dVar = 0.5 * screenWidth;
            AppLib.SetAppGlobalValue("maxDialogWindowWidth", dVar);

            // РАЗМЕРЫ ШРИФТОВ
            double appFontSize0, appFontSize1, appFontSize2, appFontSize3, appFontSize4, appFontSize5, appFontSize6, appFontSize7;
            double minVal = Math.Min(screenWidth, screenHeight);
            appFontSize0 = 0.055 * minVal;
            appFontSize1 = 0.04 * minVal;
            appFontSize2 = 0.8 * appFontSize1;
            appFontSize3 = 0.8 * appFontSize2;
            appFontSize4 = 0.8 * appFontSize3;
            appFontSize5 = 0.8 * appFontSize4;
            appFontSize6 = 0.8 * appFontSize5;
            appFontSize7 = 0.8 * appFontSize6;
            AppLib.SetAppGlobalValue("appFontSize0", appFontSize0);
            AppLib.SetAppGlobalValue("appFontSize1", appFontSize1);
            AppLib.SetAppGlobalValue("appFontSize2", appFontSize2);
            AppLib.SetAppGlobalValue("appFontSize3", appFontSize3);
            AppLib.SetAppGlobalValue("appFontSize4", appFontSize4);
            AppLib.SetAppGlobalValue("appFontSize5", appFontSize5);
            AppLib.SetAppGlobalValue("appFontSize6", appFontSize6);
            AppLib.SetAppGlobalValue("appFontSize7", appFontSize7);
        }

        #endregion

    }
}
