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


namespace WpfClient
{
    public static class AppLib
    {
        // общий логгер
        private static NLog.Logger AppLogger;
        private static string _appPromoCode;

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

        #region App logger

        public static void WriteLogTraceMessage(string msg)
        {
            if ((bool)AppLib.GetAppGlobalValue("IsWriteTraceMessages")) AppLogger.Trace(msg);
        }
        public static void WriteLogTraceMessage(string format, params string[] values)
        {
            if ((bool)AppLib.GetAppGlobalValue("IsWriteTraceMessages")) AppLogger.Trace(format, values);
        }

        public static void WriteLogInfoMessage(string msg)
        {
            AppLogger.Info(msg);
        }
        public static void WriteLogInfoMessage(string format, params string[] values)
        {
            AppLogger.Info(format, values);
        }

        public static void WriteLogErrorMessage(string msg)
        {
            AppLogger.Error(msg);
        }
        public static void WriteLogErrorMessage(string format, params string[] values)
        {
            AppLogger.Error(format, values);
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

        #endregion

        #region app settings
        // получить настройки приложения из config-файла
        public static string GetAppSetting(string key)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Any(k => k == key) == true)
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

        public static void ShowMessage(string title, string message, int closeInterval = 0)
        {
            WpfClient.Lib.MsgBoxExt mBox = new WpfClient.Lib.MsgBoxExt()
            {
                Title = title,
                TitleFontSize = (double)AppLib.GetAppGlobalValue("appFontSize6"),

                MsgBoxButton = MessageBoxButton.OK,
                ButtonsText = "Ok",
                ButtonFontSize = (double)AppLib.GetAppGlobalValue("appFontSize4"),
                ButtonForeground = Brushes.White,
                ButtonBackground = (Brush)AppLib.GetAppGlobalValue("appBackgroundColor"),
                ButtonBackgroundOver = (Brush)AppLib.GetAppGlobalValue("appBackgroundColor"),

                MessageText = message,
                MessageFontSize = (double)AppLib.GetAppGlobalValue("appFontSize2")

            };
            if (closeInterval != 0) mBox.AutoCloseInterval = closeInterval;
            
            mBox.ShowDialog();

            mBox = null;
        }

        #endregion

        #region AppBL

        // сохранить настройки приложения из config-файла в свойствах приложения
        private static bool saveAppSettingToProps(string settingName, Type settingType = null)
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
            // прочие настройки
            saveAppSettingToProps("ssdID", null);   // идентификатор устройства самообслуживания
            saveAppSettingToProps("CurrencyChar", null);   // символ денежной единицы
            saveAppSettingToProps("UserIdleTime", typeof(int));        // время бездействия из config-файла, в сек
            // печать чека
            saveAppSettingToProps("BillPageWidht", typeof(int));   // ширина в пикселях, для перевода в см надо / на 96 и * на 2,45
            saveAppSettingToProps("AutoCloseMsgBoxAfterPrintOrder", typeof(int));   // время показа информац.окна после печати
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
            saveAppSettingToPropTypeBool("IsWriteTraceMessages");

            // добавить некоторые постоянные тексты (заголовки, надписи на кнопках)
            parseAndSetAllLangString("dialogBoxYesText");
            parseAndSetAllLangString("dialogBoxNoText");
            parseAndSetAllLangString("cartDelIngrTitle");
            parseAndSetAllLangString("cartDelIngrQuestion");
            parseAndSetAllLangString("cartDelDishTitle");
            parseAndSetAllLangString("cartDelDishQuestion"); 
            parseAndSetAllLangString("printOrderTitle"); 
            parseAndSetAllLangString("printOrderErrorMessage"); 

            parseAndSetAllLangString("wordOr");
            parseAndSetAllLangString("wordIngredients");
            
            parseAndSetAllLangString("takeOrderOut");
            parseAndSetAllLangString("takeOrderIn");
            parseAndSetAllLangString("CurrencyName");
            parseAndSetAllLangString("withGarnish");
            parseAndSetAllLangString("areYouHereQuestion");
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
                        AppLib.WriteLogTraceMessage("- параметр "+ item.UniqName + "...");

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

                        AppLib.WriteLogTraceMessage("- параметр " + item.UniqName + "... Ready");
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

        // очистка заказа, закрытие всех окон и возврат в начальный экран
        public static void ReDrawApp(bool isResetLang, bool isCloseChildWindow)
        {
            if (isCloseChildWindow == true) CloseChildWindows();

            WpfClient.MainWindow mainWin = (WpfClient.MainWindow)Application.Current.MainWindow;
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

            // очистить заказ
            OrderItem curOrder = (OrderItem)AppLib.GetAppGlobalValue("currentOrder");
            if (curOrder != null) curOrder.Clear();
            mainWin.updatePrice();
        }

        // закрыть все открытые окна, кроме главного окна
        public static void CloseChildWindows()
        {
            foreach (Window win in Application.Current.Windows)
            {
                if ((win is WpfClient.MainWindow) == false)
                {
                    Type t = win.GetType();
                    PropertyInfo pInfo = t.GetProperty("Host");
                    if (pInfo == null) win.Close();
                }
            }
            
        }

        #endregion

        #region работа с промокодом

        public static bool ShowPromoCodeWindow()
        {
            Promocode promoWin = new Promocode(AppLib.GetPromoCode());
            bool isOk = (promoWin.ShowDialog() ?? false);

            if (isOk) SetPromoCode(promoWin.InputValue);

            promoWin = null;
            GC.Collect();

            return isOk;
        }

        public static void SetPromoCode(string value)
        {
            _appPromoCode = value;
        }
        public static string GetPromoCode()
        {
            return _appPromoCode;
        }

        public static void SetPromoCodeTextBlock(TextBlock textBlock)
        {
            if (textBlock == null) return;

            if (string.IsNullOrEmpty(GetPromoCode()) == true)
            {
                textBlock.Text = AppLib.GetLangTextFromAppProp("invitePromoText");
                textBlock.Style = (Style)App.Current.Resources["promoInviteTextStyle"];
                textBlock.FontSize = (double)AppLib.GetAppGlobalValue("appFontSize5");
            }
            else
            {
                textBlock.Text = GetPromoCode();
                textBlock.Style = (Style)App.Current.Resources["promoCodeTextStyle"];
                textBlock.FontSize = (double)AppLib.GetAppGlobalValue("appFontSize4");
            }
        }

        #endregion

    }  // class

}
