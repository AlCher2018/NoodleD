﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

using AppModel;
using UserActionLog;
using AppActionNS;
using WpfClient.Lib;
using System.Data.Entity;
using System.Data.SqlClient;
using WpfClient.Views;

namespace WpfClient
{
    public static class AppLib
    {
        // общий логгер
        private static NLog.Logger AppLogger;

        public static bool IsDrag, IsEventsEnable;
        public static double ScreenScale = 1d;

        // вспомогательные окна
        public static Promocode PromoCodeWindow = null;
        public static TakeOrder TakeOrderWindow = null;
        public static MsgBoxExt MessageWindow = null;
        public static MsgBoxExt ChoiceWindow = null;

        /// <summary>
        /// Static Ctor
        /// </summary>
        static AppLib()
        {
            // логгер приложения
            AppLogger = NLog.LogManager.GetCurrentClassLogger();
        }

        public static void OnClosingApp()
        {
            UserActionsLog actionIdle = (UserActionsLog)AppLib.GetAppGlobalValue("actionIdle");
            if (actionIdle != null)
            {
                actionIdle.FinishLoggingUserActions();
            }

        }

        public static void RestartApplication(string args = null)
        {
            System.Diagnostics.Process curProcess = System.Diagnostics.Process.GetCurrentProcess();

            System.Diagnostics.ProcessStartInfo pInfo = new System.Diagnostics.ProcessStartInfo();
            //pInfo.Arguments = string.Format("/C \"{0}\"", System.Reflection.Assembly.GetExecutingAssembly().Location);
            //pInfo.FileName = "cmd.exe";
            pInfo.FileName = System.Reflection.Assembly.GetExecutingAssembly().Location;
            if (args.IsNull() == false) pInfo.Arguments = args;

            System.Diagnostics.Process.Start(pInfo);

            curProcess.Kill();
        }

        #region bitwise
        public static void SetBit(ref int bitMask, int bit)
        {
            bitMask |= (1 << bit);
        }
        public static void ClearBit(ref int bitMask, int bit)
        {
            bitMask &= ~(1 << bit);
        }
        public static bool IsSetBit(int bitMask, int bit)
        {
            int val = (1 << bit);
            return (bitMask & val) == val;
        }
        #endregion

        #region App logger

        public static void WriteLogTraceMessage(string msg)
        {
            if (AppLib.GetAppSetting("IsWriteTraceMessages").ToBool()) AppLogger.Trace(" " + msg);
        }

        public static void WriteLogInfoMessage(string msg)
        {
            AppLogger.Info(msg);
        }

        public static void WriteLogErrorMessage(string msg)
        {
            AppLogger.Error(msg);
        }
        
        public static void WriteAppAction(string formName, AppActionsEnum actionType, string value = null)
        {
            // если надо, создать заказ в Главном окне
            string mainWinName = App.Current.MainWindow.Name;
            if ((formName == mainWinName) && 
                ((actionType != AppActionsEnum.MainWindowOpen) && (actionType != AppActionsEnum.MainWindowClose)))
            {
                MainWindow mainWin = (App.Current.MainWindow as MainWindow);
                if (mainWin.CurrentOrder == null) mainWin.CurrentOrder = AppLib.CreateNewOrder();
            }

            if ((bool)AppLib.GetAppGlobalValue("IsLogUserAction") == false) return;

            App.AppActionLogger.AddAction(new AppActionNS.UICAction()
            {
                deviceId = App.DeviceId,
                orderNumber = App.OrderNumber,
                formName = formName,
                actionType = actionType,
                value = value
            });
        }

        #endregion

        #region system info
        // in Mb
        public static int getAvailableRAM()
        {
            int retVal = 0;

            // class get memory size in kB
            System.Management.ManagementObjectSearcher mgmtObjects = new System.Management.ManagementObjectSearcher("Select * from Win32_OperatingSystem");
            foreach (var item in mgmtObjects.Get())
            {
                //System.Diagnostics.Debug.Print("FreePhysicalMemory:" + item.Properties["FreeVirtualMemory"].Value);
                //System.Diagnostics.Debug.Print("FreeVirtualMemory:" + item.Properties["FreeVirtualMemory"].Value);
                //System.Diagnostics.Debug.Print("TotalVirtualMemorySize:" + item.Properties["TotalVirtualMemorySize"].Value);
                retVal = (Convert.ToInt32(item.Properties["FreeVirtualMemory"].Value)) / 1024;
            }
            return retVal;
        }

        internal static bool CheckDBConnection(Type dbType)
        {
            AppLib.WriteLogTraceMessage("- проверка доступа к базе данных...");

            // контекст БД
            DbContext dbContext = (DbContext)Activator.CreateInstance(dbType);

            SqlConnection dbConn = (SqlConnection)dbContext.Database.Connection;
            AppLib.WriteLogTraceMessage("-- строка подключения: " + dbConn.ConnectionString);

            // создать такое же подключение, но с TimeOut = 1 сек
            SqlConnectionStringBuilder confBld = new SqlConnectionStringBuilder(dbConn.ConnectionString);
            SqlConnectionStringBuilder testBld = new SqlConnectionStringBuilder()
            {
                DataSource = confBld.DataSource,
                InitialCatalog = confBld.InitialCatalog,
                PersistSecurityInfo = confBld.PersistSecurityInfo,
                IntegratedSecurity = confBld.IntegratedSecurity,
                UserID = confBld.UserID,
                Password = confBld.Password,
                ConnectRetryCount = 1,
                ConnectTimeout = 1
            };
            SqlConnection testConn = new SqlConnection(testBld.ConnectionString);
            bool retVal = false;
            try
            {
                testConn.Open();
                retVal = true;
            }
            catch (Exception ex)
            {
                AppLib.WriteLogErrorMessage("--- ошибка доступа к БД: " + ex.Message);
            }
            finally
            {
                testConn.Close();
                testConn = null;
            }

            AppLib.WriteLogTraceMessage("- проверка доступа к базе данных - " + ((retVal) ? "READY" :"ERROR!!!"));
            return retVal;
        }

        public static string GetAppFileName()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            return assembly.ManifestModule.Name;
        }

        public static string GetAppFullFile()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            return assembly.Location;
        }

        public static string GetAppDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string GetFullFileName(string fileName)
        {
            return getImagePath() + fileName; 
        }
        private static string getImagePath()
        {
            string imgPath = (string)AppLib.GetAppGlobalValue("ImagesPath");
            if (string.IsNullOrEmpty(imgPath))  // путь не указан в конфиге - берем путь приложения
                imgPath = AppLib.GetAppDirectory();
            else if (imgPath.Contains(@"\:"))   // абсолютный путь
            { }
            else  // относительный путь
            {
                imgPath = AppLib.GetAppDirectory() + imgPath;
            }
            if (imgPath.EndsWith(@"\") == false) imgPath += @"\";
            return imgPath;
        }
        #endregion

        #region app settings
        // получить настройки приложения из config-файла
        public static string GetAppSetting(string key)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Any(k => k.ToLower().Equals(key.ToLower())) == true)
                return ConfigurationManager.AppSettings.Get(key);
            else
                return null;
        }

        // возвращает ресурс приложения по имени ресурса, если ресурс не найден, возвращает null
        public static object GetAppResource(string nameResource)
        {
            return Application.Current.TryFindResource(nameResource);
        }

        // return "ru", "ua" or "en"
        public static string AppLang {
            get { return (string)GetAppGlobalValue("currentAppLang"); }
            set { SetAppGlobalValue("currentAppLang", (string)value); }
        }

        public static string GetLangText(Dictionary<string,string> langDict)
        {
            string retVal = null;
            if (langDict == null) return retVal;

            string langId = AppLang;
            if (langDict.TryGetValue(langId, out retVal) == false) retVal = "no value";
            return retVal;
        }

        public static string GetLangTextFromAppProp(string key)
        {
            var dict = GetAppGlobalValue(key);
            if (dict is Dictionary<string, string>)
            {
                return GetLangText(dict as Dictionary<string, string>);
            }
            else return null;
        }

        // получить глобальное значение приложения из его свойств
        public static object GetAppGlobalValue(string key, object defaultValue = null)
        {
            IDictionary dict = Application.Current.Properties;
            if (dict.Contains(key) == false) return defaultValue;
            else return dict[key];
        }
        
        // установить глобальное значение приложения (в свойствах приложения)
        public static void SetAppGlobalValue(string key, object value)
        {
            IDictionary dict = Application.Current.Properties;
            if (dict.Contains(key) == false)  // если еще нет значения в словаре
            {
                dict.Add(key, value);   // то добавить
            }
            else    // иначе - изменить существующее
            {
                dict[key] = value;
            }
        }


        public static object GetUIElementFromPanel(System.Windows.Controls.Panel panel, string nameChild)
        {
            IEnumerable<FrameworkElement> iList = panel.Children.Cast<FrameworkElement>();
            return iList.FirstOrDefault(el => el.Name == nameChild);
        }

        #endregion

        #region WPF UI interface

        public static void SelectListBoxItemByHisInnerControl(FrameworkElement sourceControl, ListBox targetListBox)
        {
            FrameworkElement lbItemContainer = AppLib.FindVisualParentByType((FrameworkElement)sourceControl, typeof(ListBoxItem));
            if (lbItemContainer == null) return;
            int idxContainer = targetListBox.ItemContainerGenerator.IndexFromContainer(lbItemContainer);
            if (idxContainer != targetListBox.SelectedIndex)
            {
                targetListBox.SelectedIndex = idxContainer;
            }
        }

        public static double GetRowHeightAbsValue(Grid grid, int iRow, double totalHeight)
        {
            double cntStars = grid.RowDefinitions.Sum(r => r.Height.Value);
            return grid.RowDefinitions[iRow].Height.Value / cntStars * totalHeight;
        }

        public static bool IsAppVerticalLayout
        {
            get
            {
                double appWidth = (double)AppLib.GetAppGlobalValue("screenWidth");
                double appHeight = (double)AppLib.GetAppGlobalValue("screenHeight");
                return (appWidth < appHeight);
            }
        }

        public static IEnumerable<T> FindLogicalChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                foreach (object rawChild in LogicalTreeHelper.GetChildren(depObj))
                {
                    if (rawChild is DependencyObject)
                    {
                        DependencyObject child = (DependencyObject)rawChild;
                        if (child is T)
                        {
                            yield return (T)child;
                        }

                        foreach (T childOfChild in FindLogicalChildren<T>(child))
                        {
                            yield return childOfChild;
                        }
                    }
                }
            }
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = (DependencyObject)VisualTreeHelper.GetChild(depObj,i);
                    if (child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static DependencyObject GetAncestorByType(DependencyObject element, Type type)
        {
            if (element == null) return null;

            if (element.GetType() == type) return element;

            return GetAncestorByType(VisualTreeHelper.GetParent(element), type);
        }

        public static FrameworkElement FindLogicalParentByName(FrameworkElement objectFrom, string parentName, int parentLevel = -1)
        {
            int iLevel = 1;
            DependencyObject parent = LogicalTreeHelper.GetParent((DependencyObject)objectFrom);
            while (parent.GetValue(FrameworkElement.NameProperty).ToString() != parentName)
            {
                iLevel++;
                if ((parentLevel != -1) && (iLevel > parentLevel)) break;
                parent = LogicalTreeHelper.GetParent(parent);
            }

            return (parent.GetValue(FrameworkElement.NameProperty).ToString() == parentName) ? (FrameworkElement)parent : null;
        }

        // synonym of GetAncestorByType
        public static FrameworkElement FindVisualParentByType(FrameworkElement objectFrom, Type findType)
        {
            if (objectFrom == null) return null;

            DependencyObject parent = objectFrom;

            while (!(parent.GetType().Equals(findType)))
            {
                parent = VisualTreeHelper.GetParent(parent);
                if (parent == null) break;
                if ((parent is Window) || (parent is Page)) { parent = null; break; }
            }

            return (parent == null)? null: parent as FrameworkElement;
        }

        public static FrameworkElement FindLogicalChildrenByName(FrameworkElement objectFrom, string childName)
        {
            if (objectFrom == null) return null;
            foreach (object rawChild in LogicalTreeHelper.GetChildren(objectFrom))
            {
                if (rawChild is FrameworkElement)
                {
                    FrameworkElement curObj = (rawChild as FrameworkElement);
                    if (curObj.Name == childName) return curObj;
                    else return FindLogicalChildrenByName(curObj, childName);
                }
            }

            return null;
        }

        #endregion

        #region диалоговые окна
        public static void CreateMsgBox()
        {
            MsgBoxExt mBox = new MsgBoxExt()
            {
                TitleFontSize = (double)AppLib.GetAppGlobalValue("appFontSize6"),
                MessageFontSize = (double)AppLib.GetAppGlobalValue("appFontSize2"),
                ButtonFontSize = (double)AppLib.GetAppGlobalValue("appFontSize4"),

                MsgBoxButton = MessageBoxButton.OK,
                ButtonsText = "Ok",
                ButtonForeground = Brushes.White,
                ButtonBackground = (Brush)AppLib.GetAppGlobalValue("appBackgroundColor"),
                ButtonBackgroundOver = (Brush)AppLib.GetAppGlobalValue("appBackgroundColor"),

                CloseByButtonPress = false
            };
            AppLib.MessageWindow = mBox;
        }
        public static void CreateChoiceBox()
        {
            MsgBoxExt chBox = new MsgBoxExt()
            {
                TitleFontSize = (double)AppLib.GetAppGlobalValue("appFontSize6"),
                MessageFontSize = (double)AppLib.GetAppGlobalValue("appFontSize2"),
                ButtonFontSize = (double)AppLib.GetAppGlobalValue("appFontSize4"),

                MsgBoxButton = MessageBoxButton.YesNo,

                ButtonForeground = Brushes.White,
                ButtonBackground = (Brush)AppLib.GetAppGlobalValue("appBackgroundColor"),
                ButtonBackgroundOver = (Brush)AppLib.GetAppGlobalValue("appBackgroundColor"),

                CloseByButtonPress = false
            };
            AppLib.ChoiceWindow = chBox;
        }


        public static void ShowMessageBox(string title, string message)
        {
            if (AppLib.MessageWindow == null) AppLib.CreateMsgBox();

            AppLib.MessageWindow.Title = title;
            AppLib.MessageWindow.MessageText = message;

            AppLib.MessageWindow.ShowDialog();
        }

        public static MessageBoxResult ShowChoiceBox(string title, string message)
        {
            if (AppLib.ChoiceWindow == null) AppLib.CreateChoiceBox();

            AppLib.ChoiceWindow.Title = title;
            AppLib.ChoiceWindow.MessageText = message;

            // надписи на кнопках Да/Нет согласно выбранному языку
            string sYes = AppLib.GetLangTextFromAppProp("dialogBoxYesText");
            string sNo = AppLib.GetLangTextFromAppProp("dialogBoxNoText");
            AppLib.ChoiceWindow.ButtonsText = string.Format(";;{0};{1}", sYes, sNo);

            MessageBoxResult retVal = AppLib.ChoiceWindow.ShowDialog();

            return retVal;
        }

        #endregion

        #region AppBL

        // сохранить настройки приложения из config-файла в свойствах приложения
        public static bool saveAppSettingToProps(string settingName, Type settingType = null)
        {
            string settingValue = AppLib.GetAppSetting(settingName);
            if (settingValue == null) return false;

            if (settingType == null)
                AppLib.SetAppGlobalValue(settingName, settingValue);   // по умолчанию сохраняется как строка
            else
            {
                MethodInfo mi = settingType.GetMethods().FirstOrDefault(m => m.Name == "Parse");
                // если у типа есть метод Parse
                if (mi != null)  // то распарсить значение
                {
                    object classInstance = Activator.CreateInstance(settingType);
                    object oVal = mi.Invoke(classInstance, new object[] { settingValue });
                    AppLib.SetAppGlobalValue(settingName, oVal);
                }
                else
                    AppLib.SetAppGlobalValue(settingName, settingValue);   // по умолчанию сохраняется как строка
            }
            return true;
        }

        public static void GetSettingsFromConfigFile()
        {
            AppLib.WriteLogTraceMessage("Читаю настройки из файла *.config ...");

            // прочие настройки
            saveAppSettingToProps("ssdID", null);   // идентификатор устройства самообслуживания
            App.DeviceId = (string)AppLib.GetAppGlobalValue("ssdID");

            saveAppSettingToProps("ImagesPath");   // путь к папке с изображениями
            saveAppSettingToProps("CurrencyChar", null);   // символ денежной единицы
            // печать чека
            saveAppSettingToProps("BillPageWidht", typeof(int));   // ширина в пикселях, для перевода в см надо / на 96 и * на 2,45
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

        public static void ReadSettingFromDB()
        {
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
                        // преобразовать и переопределить цвета
                        if (item.UniqName.EndsWith("Color") == true)
                        {
                            convertAppColor(item.UniqName);
                            checkAppColor(item.UniqName);
                        }

                        // заменить в Application.Properties строку StringValue на словарь языковых строк
                        if (item.Value == "StringValue")    
                        {
                            Dictionary<string, string> d = getLangTextDict(stringTable, item.RowGUID, 1);
                            AppLib.SetAppGlobalValue(item.UniqName, d);
                        }

//                        AppLib.WriteLogTraceMessage("- параметр " + item.UniqName + "... Ready");
                    }
                }
                catch (Exception e)
                {
                    AppLib.WriteLogErrorMessage(string.Format("Fatal error: {0}\nSource: {1}\nStackTrace: {2}", e.Message, e.Source, e.StackTrace));
                    MessageBox.Show("Ошибка доступа к данным: " + e.Message + "\nПрограмма будет закрыта.");
                    throw;
                }
            }
            AppLib.WriteLogTraceMessage("Получаю настройки приложения из таблицы Setting ... READY");

        }

        public static void ReadAppDataFromDB()
        {
            AppLib.WriteLogTraceMessage("Получаю из MS SQL главное меню...");

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

        public static Brush GetSolidColorBrushFromAppProps(string propName, Brush defaultBrush)
        {
            string sProp = (string)GetAppSetting(propName);
            if (sProp == null)
            {
                return defaultBrush;
            }
            else
            {
                var color = ColorConverter.ConvertFromString(sProp);
                if (color == null) return defaultBrush;
                else return new SolidColorBrush((Color)color);
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

        public static string GetCostUIText(decimal cost)
        {
            string orderPriceText = cost.ToString("0");

            string currencyChar = AppLib.GetAppGlobalValue("CurrencyChar") as string;
            if (currencyChar != null) orderPriceText += " " + currencyChar;

            return orderPriceText;
        }

        public static DishItem GetDishItemByRowGUID(string rowGuid)
        {
            IEnumerable<AppModel.MenuItem> mFolders = (IEnumerable<AppModel.MenuItem>)AppLib.GetAppGlobalValue("mainMenu");
            DishItem retVal;
            foreach (AppModel.MenuItem menuItem in mFolders)
            {
                if ((retVal = menuItem.Dishes.FirstOrDefault<DishItem>(d => d.RowGUID.ToString() == rowGuid)) != null) return retVal;
            }
            return null;
        }


        #region работа с заказом
        public static OrderItem CreateNewOrder()
        {
            string deviceName = (string)AppLib.GetAppGlobalValue("ssdID", string.Empty);
            int rndFrom = int.Parse(AppLib.GetAppSetting("RandomOrderNumFrom"));       // случайный номер заказа: От
            int rndTo = int.Parse(AppLib.GetAppSetting("RandomOrderNumTo"));           // случайный номер заказа: До

            OrderItem order = new OrderItem() {
                DeviceID = deviceName,
                RangeOrderNumberFrom = rndFrom, RangeOrderNumberTo = rndTo
            };

            DateTime? dtOrder;
            // создать случайный номер заказа и получить дату заказа из БД
            order.CreateOrderNumberForPrint(out dtOrder);  // 
            order.OrderDate = dtOrder;
            App.OrderNumber = order.OrderNumberForPrint.ToString();

            // сохранить ссылку на новый заказ в глоб.перем.
            AppLib.SetAppGlobalValue("currentOrder", order);
            // и в Главном окне
            MainWindow mainWin = (App.Current.MainWindow as MainWindow);
            mainWin.CurrentOrder = order;

            AppLib.WriteAppAction("App", AppActionsEnum.CreateNewOrder, order.OrderNumberForPrint.ToString()+";"+string.Format("{0:yyyy-MM-dd}", order.OrderDate));

            return order;
        }
        public static OrderItem GetCurrentOrder()
        {
            OrderItem order = (OrderItem)GetAppGlobalValue("currentOrder");
            if (order == null) order = AppLib.CreateNewOrder();

            return order;
        }
        #endregion

        // закрытие всех окон и возврат в начальный экран
        // создание нового заказа
        public static void ReStartApp(bool isResetLang, bool isCloseChildWindow, bool isCreateNewOrder)
        {
            if (isCloseChildWindow == true) CloseChildWindows();

            MainWindow mainWin = (MainWindow)Application.Current.MainWindow;
            mainWin.ClearSelectedGarnish();
            mainWin.HideDishesDescriptions();

            mainWin.lstMenuFolders.SelectedIndex = 0;
            mainWin.scrollDishes.ScrollToTop();

            // установить язык UI
            if (isResetLang == true)
            {
                string langDefault = AppLib.GetAppSetting("langDefault");
                mainWin.selectAppLang(langDefault);
            }

            // заказ
            OrderItem order;
            if (isCreateNewOrder)
            {
                order = CreateNewOrder();
            }
            else
            {
                order = AppLib.GetCurrentOrder();
                order.Clear();
            }

            mainWin.updatePrice();
        }

        public static bool IsOpenWindow(string typeName, string objName = null)
        {
            bool retVal = false;
            foreach (Window win in App.Current.Windows)
            {
                if ((win.GetType().Name.Equals(typeName)) && (string.IsNullOrEmpty(objName) ? true : win.Name.Equals(objName)))
                {
                    retVal = (win.Visibility == Visibility.Visible);
                    break;
                }
            }

            return retVal;
        }

        // закрыть все открытые окна, кроме главного окна
        // проще перечислить, какие надо закрывать, а какие прятать
        public static void CloseChildWindows(bool isCloseAuxWindows = false)
        {
            Window currWin;
            string[] aTypesClose = new string[] {"Cart","DishPopup"};
            string[] aTypesHide = new string[] {"Promocode", "TakeOrder"};

            foreach (Window win in App.Current.Windows)
            {
                currWin = win;
                // TODO протестить MsgBoxExt на закрытие и прятание
                Type winType = currWin.GetType();
                PropertyInfo pInfo = winType.GetProperty("Host");
                if (pInfo == null)
                {
                    if ((aTypesClose.Contains(winType.Name)) ||
                        ((winType.Name == "MsgBoxExt") && ((currWin as MsgBoxExt).CloseByButtonPress == true)))
                    {
                        currWin.Close();
                        currWin = null;
                    }
                    else if ((aTypesHide.Contains(winType.Name)) ||
                        ((winType.Name == "MsgBoxExt") && ((currWin as MsgBoxExt).CloseByButtonPress == false)))
                    {
                        if (isCloseAuxWindows == true)
                        {
                            currWin.Close();
                            currWin = null;
                        }
                        else if (currWin.Visibility == Visibility.Visible)
                        {
                            currWin.Hide();
                        }
                    }
                }
            }  // for each
            
        }  // method

        #endregion

        // текст на элементе промокода
        public static void SetPromocodeTextStyle(TextBlock txtInput)
        {
            if (string.IsNullOrEmpty(App.PromocodeNumber))
            {
                txtInput.Text = AppLib.GetLangTextFromAppProp("invitePromoText");
                txtInput.Style = (Style)App.Current.Resources["promoInviteTextStyle"];
                txtInput.FontSize = (double)AppLib.GetAppGlobalValue("appFontSize5");
            }
            else
            {
                txtInput.Text = App.PromocodeNumber;
                txtInput.Style = (Style)App.Current.Resources["promoCodeTextStyle"];
                txtInput.FontSize = (double)AppLib.GetAppGlobalValue("appFontSize4");
            }
        }

    }  // class

}
