using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using AppModel;
using WpfClient.Lib;
using System.Xml.Serialization;
using IntegraLib;

namespace WpfClient.Model
{
    public class PrintBill
    {
        private OrderItem _order;
        string _langId;

        public PrintBill(OrderItem order)
        {
            _order = order;
            _langId = AppLib.AppLang;
        }

        public bool CreateBill(out string msgBoxText)
        {
            bool retVal = true;
            msgBoxText = null;
            string userErrMsgSuffix = Environment.NewLine + AppLib.GetLangTextFromAppProp("userErrMsgSuffix");
            userErrMsgSuffix = userErrMsgSuffix.Replace("\\n", Environment.NewLine);

            AppLib.WriteLogTraceMessage("Создание пречека для заказа " + _order.OrderNumberForPrint.ToString());

            // свойства заказа, созадаваемые перед печатью чека:
            //      1. BarCodeValue - значение штрих-кода, 12 цифр (6 - yymmdd, 2 - код источника, 4 - номер чека для печати)
            //      2. LanguageTypeId - язык, который был выбран при создании чека (ua/en/ru)
            //--------------------------------------------------

            string deviceName = (string)AppLib.GetAppGlobalValue("ssdID", string.Empty);
            if (deviceName == string.Empty)
            {
                AppLib.WriteLogErrorMessage("В config-файле не найден элемент \"ssdID\" - идентификатор терминала самообслуживания.\n\t\tTrace: PrintBill.cs, CreateBill()");
                msgBoxText = "App config error: don't find 'ssdID' element." + userErrMsgSuffix;
                return false;
            }
            if (deviceName.Length > 2) deviceName = deviceName.Substring(0, 2);

            // 1. OrderNumberForPrint
            if (_order.OrderNumberForPrint == -1)
            {
                AppLib.WriteLogErrorMessage("Класс PrintBill. Не указан номер заказа");
                msgBoxText = "App error: no order number." + userErrMsgSuffix;
                return false;
            }

            // дату заказа создаем ПЕРЕД печатью
            _order.OrderDate = DateTime.Now;

            // 2. BarCodeValue
            //    идент.устройства - 2 символа
            if (deviceName.Length <= 2)
                deviceName = string.Format("{0:D2}", deviceName);
            else
                deviceName = deviceName.Substring(0, 2);
            //    дата заказа в формате yyMMdd - 6 символов
            //    номер заказа (для печати - случайный) в формате 0000 - 4 символа
            // т.к. в формате EAN-13 если первый символ - 0, то он удаляется, используем в начале дату
            string sBuf = (_order.OrderDate==null) ? "000000" : string.Format("{0:yyMMdd}", _order.OrderDate);
            _order.BarCodeValue = sBuf + _order.OrderNumberForPrint.ToString("000000");

            // 3. LanguageTypeId
            _order.LanguageTypeId = AppLib.AppLang;

            // ширина из config-файла
            int width = (int)AppLib.GetAppGlobalValue("BillPageWidht", 0);
            if (width == 0)
            {
                AppLib.WriteLogErrorMessage("В config-файле не указан элемент BillPageWidht с шириной чека. Берется значение по умолчанию - 300 (7,8см)");
                width = 300;
            }

            // принтеры в системе
            List<PrintQueue> printers = PrintHelper.getPrintersList();
            if (printers == null)
            {
                AppLib.WriteLogErrorMessage("В системе не зарегистрирован ни один принтер!!");
                msgBoxText = AppLib.GetLangTextFromAppProp("printConfigError");
                return false;
            }
            else
            {
                try
                {
                    string sLog = string.Join(Environment.NewLine + "\t", printers.Select(pq => pq.Name + ", status '" + getPrinterStatus(pq) + "', driver '" + pq.QueueDriver.Name + "'"));
                    AppLib.WriteLogTraceMessage("Системные принтеры: " + Environment.NewLine + "\t" + sLog);
                }
                catch (Exception) { }
            }

            // имя принтера для печати чека
            string printerName = null;
            string result = null;
            #region поиск принтера для печати чека
            printerName = AppLib.GetAppSetting("PrinterName");
            PrintQueue printer = PrintHelper.GetPrintQueueByName(printerName);
            if (printer == null)
            {
                AppLib.WriteLogErrorMessage("В config-файле не указан элемент PrinterName или в системе не найден принтер: " + printerName);
                printerName = null;
            }
            else
            {
                result = getPrinterStatus(printer);
                AppLib.WriteLogTraceMessage($"Принтер '{printerName}' находится в состоянии '{result}'");
            }

            // если принтер из настроек не Ок, то берем принтер по умолчанию
            if ((printer == null) || ((result != null) && (result != "OK")))
            {
                AppLib.WriteLogTraceMessage("Предпринимается попытка использовать принтер по умолчанию...");
                printer = PrintHelper.GetDefaultPrinter();
                if ((printer != null) 
                    && ((printerName == null) || ((printerName != null) && (printer.Name != printerName))))
                {
                    printerName = printer.Name;
                    result = getPrinterStatus(printer);
                    AppLib.WriteLogTraceMessage($"Найден принтер по умолчанию: {printerName}");
                    AppLib.WriteLogTraceMessage($"Принтер '{printerName}' находится в состоянии '{result}'");
                }
            }

            // если принтер по умолчанию не ОК, то берем первый в системе
            if (printer == null)
            {
                AppLib.WriteLogTraceMessage("Предпринимается попытка использовать первый найденный принтер в ОС...");
                printer = PrintHelper.GetFirstPrinter();
                if ((printer != null)
                    && ((printerName == null) || ((printerName != null) && (printer.Name != printerName))))
                {
                    printerName = printer.Name;
                    result = getPrinterStatus(printer);
                    AppLib.WriteLogTraceMessage($"Найден первый принтер: {printerName}");
                    AppLib.WriteLogTraceMessage($"Принтер '{printerName}' находится в состоянии '{result}'");
                }
            }
            // принтер не найден - досвидос
            if (printer == null)
            {
                msgBoxText = "App print error: not found any printer." + userErrMsgSuffix;
                return false;
            }
            // найден, но статус не ОК - досвидос
            else if ((result != null) && (result != "OK"))
            {
                string sFormat = AppLib.GetLangTextFromAppProp("printerStatusMsg");
                if (sFormat != null) sFormat = sFormat.Replace("\\n", Environment.NewLine);
                msgBoxText = string.Format(sFormat, printerName, result) + userErrMsgSuffix;
                return false;
            }
            #endregion

            // создание документа
            AppLib.WriteLogTraceMessage($" Создаю документ для печати...");
            FlowDocument doc = null;
            try
            {
                doc = createDocument(width);
                AppLib.WriteLogTraceMessage($" - документ создан успешно");
            }
            catch (Exception ex)
            {
                result = AppLib.GetLangTextFromAppProp("afterPrintingErrMsg");
                if (result != null) result = result.Replace("\\n", Environment.NewLine);
                msgBoxText = result + userErrMsgSuffix;
                AppLib.WriteLogErrorMessage(" Ошибка формирования документа: " + ex.ToString());
                return false;
            }

            // имя задания на принтер
            string prnTaskName = "bill " + _order.OrderNumberForPrint.ToString();
            // вывод документа на принтер
            AppLib.WriteLogTraceMessage($" Вывожу пречек на принтер...");
            retVal = PrintHelper.PrintFlowDocument(doc, prnTaskName, printerName, out msgBoxText);
            if (retVal == false)
            {
                // сообщение об ошибке в лог
                AppLib.WriteLogErrorMessage(" Ошибка печати документа: " + msgBoxText);
                // и на экран пользователю
                result = AppLib.GetLangTextFromAppProp("afterPrintingErrMsg");
                if (result != null) result = result.Replace("\\n", Environment.NewLine);
                msgBoxText = result + userErrMsgSuffix;
            }
            else
            {
                AppLib.WriteLogTraceMessage("Пречек распечатан успешно");
                if (msgBoxText != null) AppLib.WriteLogErrorMessage(msgBoxText);
            }

            return retVal;
        }

        private string getPrinterStatus(PrintQueue printer)
        {
            string retVal = PrintQueueStatus.None.ToString();
            if (printer == null) return retVal;

            if ((printer.QueueStatus == PrintQueueStatus.None)
                || printer.FullName.Contains("PDF")
                || printer.IsXpsDevice)
                retVal = "OK";
            else
                retVal = printer.QueueStatus.ToString().ToUpper();

            return retVal;
        }

        private FlowDocument createDocument(int width)
        {
            // создать объекты верхнего и нижнего колонтитулов
            XmlDocument xmlHeader = new XmlDocument();
            xmlHeader.Load(AppDomain.CurrentDomain.BaseDirectory + string.Format(@"PrinterBill\Header-{0}.xml", _langId));
            XmlDocument xmlFooter = new XmlDocument();
            xmlFooter.Load(AppDomain.CurrentDomain.BaseDirectory + string.Format(@"PrinterBill\Footer-{0}.xml", _langId));
            TextModel textHeader = new TextModel();
            TextModel textFooter = new TextModel();
            textHeader = DeSerialize<TextModel>(xmlHeader.OuterXml);
            textFooter = DeSerialize<TextModel>(xmlFooter.OuterXml);
            int leftMargin = getLineMargin("BillLineLeftMargin");
            Thickness lineMargin = getLineMargin();
            Thickness lineMarginIngr = lineMargin;
            lineMarginIngr.Top = getLineMargin("BillLineIngrTopMargin");
            Thickness lineMarginPrice = lineMargin;
            lineMarginPrice.Top = getLineMargin("BillLinePriceTopMargin");

            var doc = new FlowDocument();
            doc.PageWidth = width;
            // значения по умолчанию
            doc.FontFamily = new FontFamily("Panton-Bold");
            doc.FontWeight = FontWeights.Normal;
            doc.FontStyle = FontStyles.Normal;
            doc.FontSize = Convert.ToInt32(AppLib.GetAppGlobalValue("BillLineFontSize", 12));

            // вставить изображение в заголовок
            addImageToDoc(textHeader, doc);
            // метка, если заказ С СОБОЙ
            if (_order.takeAway == true)
            {
                string langText = AppLib.GetLangTextFromAppProp("takeOrderOut");
                langText = string.Concat(" **** ", langText.ToUpper(), " ****");
                addParagraph(doc, langText, 1.5 * doc.FontSize, FontWeights.Bold, FontStyles.Normal, new Thickness(leftMargin, 20, 0, 10), TextAlignment.Center);
            }
            // добавить форматированный заголовок
            addSectionToDoc(textHeader, doc);

            // добавить строки заказа
            string currencyName = AppLib.GetLangTextFromAppProp("CurrencyName");
            string sAppSet;
            decimal totalPrice = 0; string itemName, stringRow;

            foreach (DishItem item in _order.Dishes)
            {
                // блюдо
                itemName = AppLib.GetLangText(item.langNames);
                // c гарниром?
                if ((item.SelectedGarnishes != null) && (item.SelectedGarnishes.Count > 0))
                {
                    DishAdding garn = item.SelectedGarnishes[0];
                    string garnName =  AppLib.GetLangText(garn.langNames);
                    // 2017-02-02 Формирование полного наименования блюда с гарниром
                    // если DishFullNameInGargnish = true, то полное имя берем из гарнира, 
                    // иначе к имени блюда добавляем имя гарнира
                    sAppSet = AppLib.GetAppSetting("DishFullNameInGarnish");
                    if (sAppSet != null && sAppSet.ToBool())
                        itemName = garnName;
                    else
                        itemName += " " + AppLib.GetLangTextFromAppProp("withGarnish") + " " + garnName;
                }
                //string stringRow = itemName.Substring(0, itemName.Count() > 30 ? 30 : itemName.Count());
                addParagraph(doc, itemName, doc.FontSize, doc.FontWeight, doc.FontStyle, lineMargin);

                // добавить ингредиенты
                if (item.SelectedIngredients != null)
                {
                    stringRow = "  + "; bool isFirst = true;
                    foreach (DishAdding ingr in item.SelectedIngredients)
                    {
                        itemName = AppLib.GetLangText(ingr.langNames);
                        stringRow += ((isFirst) ? "" : "; ") + itemName;
                        isFirst = false;
                    }
                    addParagraph(doc, stringRow, 0.9 * doc.FontSize, doc.FontWeight, FontStyles.Italic, lineMarginIngr);
                }

                // стоимость блюда
                decimal price = item.GetPrice();
                string priceString = string.Format("{0} x {1:0.00} {3} = {2:0.00} {3}", item.Count, price, item.Count * price, currencyName);
                addParagraph(doc, priceString, doc.FontSize, doc.FontWeight, doc.FontStyle, lineMarginPrice, TextAlignment.Right);
                totalPrice += item.Count * price;
            }
            // итог
            addTotalLine(doc, doc.FontSize, totalPrice, currencyName, leftMargin);

            // добавить форматированный "подвал"
            addSectionToDoc(textFooter, doc);
            // вставить изображение в "подвал"
            addImageToDoc(textFooter, doc);

            // печать штрих-кода
            string bcVal13 = _order.BarCodeValue + BarCodeLib.getUPCACheckDigit(_order.BarCodeValue);
            Image imageBarCode = BarCodeLib.GetBarcodeImage(bcVal13, (int)(1.2 * doc.PageWidth), 50);
            BlockUIContainer bcContainer = new BlockUIContainer()
            {
                Child = imageBarCode,
                Margin = new Thickness(leftMargin,10,0,0)
            };
            doc.Blocks.Add(bcContainer);
            // вывести значение баркода в чек
            //string bcDisplay = string.Format("{0} {1} {2} {3}", bcVal13.Substring(0,2), bcVal13.Substring(2, 6), bcVal13.Substring(8, 4), bcVal13.Substring(12,1));
            string bcDisplay = string.Format("{0}  {1}  {2}", bcVal13.Substring(0, 1), bcVal13.Substring(1, 6), bcVal13.Substring(7));
            addParagraph(doc, bcDisplay, 0.75 * doc.FontSize, doc.FontWeight, doc.FontStyle, new Thickness(leftMargin,5,0,0), TextAlignment.Center);

            return doc;
        }

        // отступ строк
        private Thickness getLineMargin()
        {
            int left = getLineMargin("BillLineLeftMargin");
            int top = getLineMargin("BillLineTopMargin");
            return new Thickness((double)left, (double)top, 0d, 0d);
        }
        private int getLineMargin(string appSettingKey)
        {
            int retVal = 0;

            object sAppSet = AppLib.GetAppGlobalValue(appSettingKey);
            if (sAppSet != null) retVal = Convert.ToInt32(sAppSet);

            return retVal;
        }

        private void addParagraph(FlowDocument doc, string text, double fontSize, 
            FontWeight fontWeight, FontStyle fontStyle, Thickness margin, TextAlignment alignment = TextAlignment.Left)
        {
            Run run = new Run(text);
            run.FontSize = fontSize;
            run.FontWeight = fontWeight;
            run.FontStyle = fontStyle;

            Paragraph par = new Paragraph(run);
            par.Margin = new Thickness(0);
            par.Padding = margin;
            par.TextAlignment = alignment;

            doc.Blocks.Add(par);
        }

        private Run getRunFromModel(ParagraphModel item, string text)
        {
            Run run = new Run(text);
            run.FontFamily = new FontFamily(item.FontFamily);
            run.FontSize = item.FontSize;

            FontStyle style = FontStyles.Normal;
            switch (item.FontStyle)
            {
                case "Italic":
                    style = FontStyles.Italic;
                    break;
                case "Oblique":
                    style = FontStyles.Oblique;
                    break;
                default:
                    style = FontStyles.Normal;
                    break;
            }

            return run;
        }

        private TResult DeSerialize<TResult>(string setObj)
        {
            var reader = new System.IO.StringReader(setObj);
            var serialazer = new System.Xml.Serialization.XmlSerializer(typeof(TResult));
            return (TResult)serialazer.Deserialize(reader);
        }

        private void addImageToDoc(TextModel sectionModel, FlowDocument doc)
        {
            if (sectionModel.ImageModel == null) return;

            BlockUIContainer imageBlock = ImageHelper.getImageBlock(sectionModel.ImageModel, doc);

            if (imageBlock != null) doc.Blocks.Add(imageBlock);
        }

        private void addSectionToDoc(TextModel sectionModel, FlowDocument doc)
        {
            foreach (ParagraphModel item in sectionModel.Paragraphs)
            {
                Paragraph par = null;
                string sBuf = null;

                // обработка строк шаблона заголовка
                if (item.Text.Contains("{OrderDate}") == true)
                {
                    sBuf = item.Text.Replace("{OrderDate}", string.Format("{0:dd.MM.yyyy HH:mm:ss}", _order.OrderDate));
                    Run inLineObj = getRunFromModel(item, sBuf);
                    par = new Paragraph(inLineObj);
                }
                else if (item.Text.Contains("{OrderNumber}") == true)
                {
                    // форматированный текст до и после номера счета
                    List<Run> runBlocks = new List<Run>();
                    Run r;

                    string[] aStr = item.Text.Split(new string[] { "{OrderNumber}" }, StringSplitOptions.RemoveEmptyEntries);
                    // текст перед номером
                    if (aStr.Length > 0)
                    {
                        sBuf = aStr[0];
                        r = getRunFromModel(item, sBuf); r.FontWeight = FontWeights.Bold;
                        runBlocks.Add(r);
                    }
                    // номер заказа
                    r = getRunFromModel(item, _order.OrderNumberForPrint.ToString());
                    r.FontSize = item.FontSize + 3; r.FontWeight = FontWeights.Bold;
                    runBlocks.Add(r);

                    // идентификатор устройства
                    if ((aStr.Length > 1) && (aStr[1].Contains("{ssdId}")))
                    {
                        sBuf = (string)AppLib.GetAppGlobalValue("ssdID");
                        if (sBuf != null)
                        {
                            aStr = aStr[1].Split(new string[] { "{ssdId}" }, StringSplitOptions.RemoveEmptyEntries);
                            // текст до идентификатора
                            if (aStr.Length > 0) runBlocks.Add(getRunFromModel(item, aStr[0]));
                            // идентификатор
                            r = getRunFromModel(item, sBuf); runBlocks.Add(r);
                            // текст после идент.
                            if (aStr.Length > 1) runBlocks.Add(getRunFromModel(item, aStr[1]));
                        }
                    }

                    par = new Paragraph();  // собрать тексты в абзац
                    par.Inlines.AddRange(runBlocks);
                }
                else
                {
                    Run inLineObj = getRunFromModel(item, item.Text);
                    par = new Paragraph(inLineObj);
                }

                // формат абзаца
                if (par != null)
                {
                    par.Margin = new Thickness(item.LeftMargin, item.TopMargin, item.RightMargin, item.ButtomMargin);
                    doc.Blocks.Add(par);
                    par = null;
                }
            }
        }

        private void addTotalLine(FlowDocument doc, double fontSize, decimal totalPrice, string currencyName, int leftMargin)
        {
            Table t = new Table();
            t.FontSize = fontSize;
            t.CellSpacing = 0;
            t.BorderThickness = new Thickness(0, 1, 0, 0);
            t.BorderBrush = new SolidColorBrush(Colors.Black);
            t.Margin = new Thickness(0);
            t.Padding = new Thickness(leftMargin, 10, 0, 0);

            // две колонки
            t.Columns.Add(new TableColumn() { Width = new GridLength(1, GridUnitType.Star) });
            t.Columns.Add(new TableColumn() { Width = new GridLength(1, GridUnitType.Star) });
            TableRowGroup rg = new TableRowGroup();
            t.RowGroups.Add(rg);

            string totalText = AppLib.GetLangText((Dictionary<string, string>)AppLib.GetAppGlobalValue("lblTotalText"));
            Run r = new Run(totalText);
            Paragraph p = new Paragraph(r);
            p.TextAlignment = TextAlignment.Left;
            TableCell totalTextCell = new TableCell();
            totalTextCell.Blocks.Add(p);

            string priceText = string.Format("{0:0.00} {1}", totalPrice, currencyName);
            r = new Run(priceText);
            r.FontWeight = FontWeights.Bold;
            p = new Paragraph(r);
            p.TextAlignment = TextAlignment.Right;
            TableCell priceTextCell = new TableCell();
            priceTextCell.Blocks.Add(p);

            TableRow tr = new TableRow();
            tr.Cells.Add(totalTextCell);
            tr.Cells.Add(priceTextCell);
            rg.Rows.Add(tr);

            doc.Blocks.Add(t);
        }
    }


    public class TextModel
    {
        public ImageModel ImageModel { get; set; }
        public List<ParagraphModel> Paragraphs = new List<ParagraphModel>();
    }
    public class ImageModel
    {
        [XmlAttribute]
        public string Source { get; set; }
        [XmlAttribute]
        public int LeftMargin { get; set; }

        [XmlAttribute]
        public int TopMargin { get; set; }

        [XmlAttribute]
        public int RightMargin { get; set; }

        [XmlAttribute]
        public int ButtomMargin { get; set; }
        [XmlAttribute]
        public int Width { get; set; }

        [XmlAttribute]
        public int Height { get; set; }
    }
    public class ParagraphModel
    {
        [XmlAttribute]
        public int LeftMargin { get; set; }

        [XmlAttribute]
        public int TopMargin { get; set; }

        [XmlAttribute]
        public int RightMargin { get; set; }

        [XmlAttribute]
        public int ButtomMargin { get; set; }
        [XmlText]
        public string Text { get; set; }
        [XmlAttribute]
        public string FontFamily { get; set; }
        [XmlAttribute]
        public int FontSize { get; set; }
        [XmlAttribute]
        public string FontStyle { get; set; }
    }

}
