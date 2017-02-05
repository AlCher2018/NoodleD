using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, Microsoft.Shell.ISingleInstanceApp
    {
        private const string Unique = "My_Unique_Application_String";

        [STAThread]
        public static void Main()
        {
            if (Microsoft.Shell.SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                AppLib.WriteLogInfoMessage("************  Start application  **************");

                // объем доступной памяти
                System.Management.ManagementObjectSearcher mgmtObjects = new System.Management.ManagementObjectSearcher("Select * from Win32_OperatingSystem");
                int freeMemory = 0;
                foreach (var item in mgmtObjects.Get())
                {
                    freeMemory = (Convert.ToInt32(item.Properties["FreeVirtualMemory"].Value)) / 1024;
                }
                AppLib.WriteLogInfoMessage(" - available memory: " + freeMemory.ToString() + " MB");
                if (freeMemory < 300)
                {
                    AppLib.WriteLogErrorMessage("Terminate program by low memory.");
                    AppLib.WriteLogInfoMessage("************  End application  ************");
                    MessageBox.Show("This computer has too low available memory.\nYou need at least 300 MB free memory.");
                    Environment.Exit(2);
                    //                Process.GetCurrentProcess().Kill();
                }

                System.Windows.SplashScreen splashScreen = new System.Windows.SplashScreen("AppImages/bg 3hor 1920x1080.png");
                splashScreen.Show(true);

                App app = new App();

                //******  СТАТИЧЕСКИЕ настройки  ******

                // ресурсы приложения
                //createappresources(app);        // определенные в приложении
                app.InitializeComponent();          // определенные в app.xaml

                // вычислить размеры, хранимые в свойствах приложения
                calculateAppSizes();

                //******  динамические настройки  ******
                // получение и сохранение внешних ресурсов приложения
                AppLib.GetSettingsFromConfigFile();     // определенные в config-файле

                // определенные в ms sql
                try
                {
                    AppLib.ReadSettingFromDB();
                    //testdata.mainproc();
                    AppLib.ReadAppDataFromDB();
                }
                catch (Exception)
                {
                    // сообщения об ошибках находятся в соотв.модулях, здесь только выход из приложения
                    Application.Current.Shutdown(1);
                }

                // проверка соединения с бд
                try
                {
                    checkDBConnection();
                }
                catch (Exception eConnDB)
                {
                    AppLib.WriteLogErrorMessage(eConnDB.Message);
                    throw;
                }

                // главное окно приложения
                WpfClient.MainWindow mainWindow = new WpfClient.MainWindow();
                app.Run(mainWindow);

                // Allow single instance code to perform cleanup operations
                Microsoft.Shell.SingleInstance<App>.Cleanup();
            }
        }

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


        #region check funcs

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
            app.Resources.Add("appColorSelectGarnish", Color.FromArgb(255, 137, 137, 137));  //< !--COLOR7 - grey-- >

            // кисти
            app.Resources.Add("appBackgroundColor", new SolidColorBrush((Color)app.Resources["appColorDarkPink"]));
            app.Resources.Add("appNotSelectedItemColor", new SolidColorBrush((Color)app.Resources["appColorWhite"]));
            app.Resources.Add("appSelectedItemColor", new SolidColorBrush((Color)app.Resources["appColorYellow"]));
            app.Resources.Add("mainMenuSelectedItemColor", new SolidColorBrush((Color)app.Resources["appColorDDarkPink"]));
            app.Resources.Add("addButtonBackgroundTextColor", new SolidColorBrush((Color)app.Resources["appColorSelectButton"]));
            app.Resources.Add("addButtonBackgroundPriceColor", new SolidColorBrush(Color.FromArgb(255, 147, 29, 63)));
            app.Resources.Add("cartButtonBackgroundColor", new SolidColorBrush((Color)app.Resources["appColorCartButtom"]));
            app.Resources.Add("selectGarnishBackgroundColor", new SolidColorBrush((Color)app.Resources["appColorSelectGarnish"]));
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
            app.Resources.Add("cornerRadiusLeft", new CornerRadiusConverter() { Side = "Left" });
            app.Resources.Add("cornerRadiusRight", new CornerRadiusConverter() { Side = "Right" });
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
            screenWidth = SystemParameters.PrimaryScreenWidth;
            //            screenWidth = SystemParameters.VirtualScreenWidth;
            screenHeight = SystemParameters.PrimaryScreenHeight;
            //            screenHeight = SystemParameters.VirtualScreenHeight;

            AppLib.SetAppGlobalValue("screenWidth", screenWidth);
            AppLib.SetAppGlobalValue("screenHeight", screenHeight);

            AppLib.SetAppGlobalValue("categoriesPanelWidth", (screenWidth / 6d * 1d));
            AppLib.SetAppGlobalValue("dishesPanelWidth", (screenWidth / 6d * 5d));


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
