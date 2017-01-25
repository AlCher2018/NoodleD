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

namespace WpfClient
{
    public class PrintBill
    {
        private const string UserErrMsgSuffix = "\nПечать чека невозможна.\nОбратитесь к администратору приложения.";

        private OrderItem _order;
        string _langId;
        TakeOrderEnum _takeMode;

        public PrintBill(OrderItem order, TakeOrderEnum takeMode)
        {
            _order = order;
            _langId = AppLib.AppLang;
            _takeMode = takeMode;
        }

        public bool CreateBill(out string errMessage)
        {
            bool retVal = true;
            errMessage = null;

            // свойства заказа, созадаваемые перед печатью чека:
            //      OrderNumberForPrint - номер чека для печати
            //      BarCodeValue - значение штрих-кода, 12 цифр (2 - код источника, 6 - yymmdd, 4 - номер чека для печати)

            // создать номер счета для ПЕЧАТНОГО чека
            string deviceName = (string)AppLib.GetAppGlobalValue("ssdID", string.Empty);
            if (deviceName == string.Empty)
            {
                AppLib.AppLogger.Error("В config-файле не найден элемент \"ssdID\" - идентификатор терминала самообслуживания.\n\t\tTrace: PrintBill.cs, CreateBill()");
                errMessage = "Ошибка конфигурации приложения!" + UserErrMsgSuffix;
                return false;
            }
            int rndFrom = int.Parse(AppLib.GetAppSetting("RandomOrderNumFrom"));       // случайный номер заказа: От
            int rndTo = int.Parse(AppLib.GetAppSetting("RandomOrderNumTo"));           // случайный номер заказа: До
            _order.CreateOrderNumberForPrint(deviceName, rndFrom, rndTo);  // в свойстве OrderNumberForPrint
            _order.OrderDate = DateTime.Now;
            if (_order.OrderNumberForPrint == -1)
            {
                AppLib.AppLogger.Error("Модуль OrderLib.cs, CreateOrderNumberForPrint() вернул -1 (признак ошибочного значения)");
                errMessage = "Ошибка создания номера заказа для печати чека." + UserErrMsgSuffix;
                return false;
            }
            // BarCodeValue
            //    идент.устройства - 2 символа
            if (deviceName.Length <= 2)
                deviceName = string.Format("{0:D2}", deviceName);
            else
                deviceName = deviceName.Substring(0, 2);
            //    дата заказа в формате yyMMdd - 6 символов
            //    номер заказа (для печати - случайный) в формате 0000 - 4 символа
            _order.BarCodeValue = deviceName + _order.OrderDate.ToString("yyMMdd") + _order.OrderNumberForPrint.ToString("0000");


            // ширина из config-файла
            int width = (int)AppLib.GetAppGlobalValue("BillPageWidht", 0);
            if (width == 0)
            {
                AppLib.AppLogger.Error("В config-файле не указан элемент BillPageWidht с шириной чека.\n\t\tМодуль PrintBill.cs, CreateBill()");
                errMessage = "Ошибка в конфигурации печати чека." + UserErrMsgSuffix;
                return false;
            }

            // имя принтера для печати чека
            string printerName = AppLib.GetAppSetting("PrinterName");
            if (printerName == null)
            {
                AppLib.AppLogger.Error("В config-файле не указан элемент PrinterName - имя принтера в ОС для печати чеков.\n\t\tМодуль PrintBill.cs, CreateBill()");
                errMessage = "Ошибка в конфигурации печати чека." + UserErrMsgSuffix;
                return false;
            }

            // создание документа
            FlowDocument doc = createDocument(width);

            // имя задания на принтер
            string prnTaskName = "bill " + _order.OrderNumberForPrint.ToString();
            // вывод документа на принтер
            retVal = PrintHelper.PrintFlowDocument(doc, prnTaskName, printerName, out errMessage);
            if (retVal == false)
            {
                AppLib.AppLogger.Error(errMessage + "\tМодуль PrintBill.cs, CreateBill()");
                errMessage = "Ошибка печати чека.\nОбратитесь к администратору приложения.";
            }

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

            var doc = new FlowDocument();
            doc.PageWidth = width;

            // значения по умолчанию
            doc.FontFamily = new FontFamily("Panton-Bold");
            doc.FontSize = 12;

            // вставить изображение в заголовок
            addImageToDoc(textHeader, doc);
            // метка, если заказ С СОБОЙ
            if (_takeMode == TakeOrderEnum.TakeAway)
            {
                string langText = AppLib.GetLangText((Dictionary<string, string>)AppLib.GetAppGlobalValue("takeOrderOut"));
                langText = string.Concat(" **** ", langText.ToUpper(), " ****");
                addParagraph(doc, langText, 20, FontWeights.Bold, FontStyles.Normal, new Thickness(0, 20, 0, 10), TextAlignment.Center);
            }
            // добавить форматированный заголовок
            addSectionToDoc(textHeader, doc);

            // добавить строки заказа
            string currencyName = AppLib.GetLangText((Dictionary<string, string>)AppLib.GetAppGlobalValue("CurrencyName"));
            decimal totalPrice = 0; string itemName, stringRow;
            foreach (DishItem item in _order.Dishes)
            {
                // блюдо
                itemName = AppLib.GetLangText(item.langNames);
                //string stringRow = itemName.Substring(0, itemName.Count() > 30 ? 30 : itemName.Count());
                addParagraph(doc, itemName);

                // стоимость блюда
                decimal price = item.GetPrice();
                string priceString = string.Format("{0} x {1:0.00} {3} = {2:0.00} {3}", item.Count, price, item.Count * price, currencyName);
                addParagraph(doc, priceString, 12, null, null, null, TextAlignment.Right);
                totalPrice += item.Count * price;

                // добавить ингредиенты
                if (item.SelectedIngredients != null)
                {
                    stringRow = "     "; bool isFirst = true;
                    foreach (DishAdding ingr in item.SelectedIngredients)
                    {
                        itemName = AppLib.GetLangText(ingr.langNames);
                        stringRow += ((isFirst) ? "" : "; ") + itemName;
                        isFirst = false;
                    }
                    addParagraph(doc, stringRow, 10, null, FontStyles.Italic);
                }
            }
            // итог
            addTotalLine(doc, totalPrice, currencyName);

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
                Margin = new Thickness(0,10,0,0)
            };
            doc.Blocks.Add(bcContainer);
            // вывести значение баркода в чек
            //string bcDisplay = string.Format("{0} {1} {2} {3}", bcVal13.Substring(0,2), bcVal13.Substring(2, 6), bcVal13.Substring(8, 4), bcVal13.Substring(12,1));
            string bcDisplay = string.Format("{0}  {1}  {2}", bcVal13.Substring(0, 1), bcVal13.Substring(1, 6), bcVal13.Substring(7));
            addParagraph(doc, bcDisplay, 9, FontWeights.Normal, FontStyles.Normal, new Thickness(0,5,0,0), TextAlignment.Center);

            return doc;
        }


        private void addParagraph(FlowDocument doc, string text, double fontSize=12, FontWeight? fontWeight = null, FontStyle? fontStyle = null, Thickness? margin = null, TextAlignment alignment = TextAlignment.Left)
        {
            Run run = new Run(text);
            run.FontSize = fontSize;
            run.FontWeight = (fontWeight == null) ? FontWeights.Normal : (FontWeight)fontWeight;
            run.FontStyle = (fontStyle == null) ? FontStyles.Normal : (FontStyle)fontStyle;

            Paragraph par = new Paragraph(run);
            par.Margin = (margin == null) ? new Thickness(0, 0, 0, 0) : (Thickness)margin;
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

            ImageModel imgModel = sectionModel.ImageModel;
            BlockUIContainer imageBlockHeader = new BlockUIContainer();
            Image imageHeader = new Image();
            imageHeader.Source = ImageHelper.GetBitmapImage(ImageHelper.GetFileNameBy(imgModel.Source));
            imageBlockHeader.Child = imageHeader;
            imageHeader.Width = imgModel.Width;
            imageHeader.Height = imgModel.Height;
            imageHeader.Margin = new Thickness(imgModel.LeftMargin, imgModel.TopMargin, imgModel.RightMargin, imgModel.ButtomMargin);
            doc.Blocks.Add(imageBlockHeader);
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
                    sBuf = item.Text.Replace("{OrderDate}", _order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    Run inLineObj = getRunFromModel(item, sBuf);
                    par = new Paragraph(inLineObj);
                }
                else if (item.Text.Contains("{OrderNumber}") == true)
                {
                    sBuf = _order.OrderNumberForPrint.ToString();
                    Run runOrderNumber = getRunFromModel(item, sBuf);
                    // переопределить формат текста из шаблона
                    runOrderNumber.FontFamily = new FontFamily("Panton-Bold");
                    runOrderNumber.FontSize = item.FontSize + 3;
                    runOrderNumber.FontWeight = FontWeights.Bold;
                    // форматированный текст до и после номера счета
                    string[] separate = Regex.Split(item.Text, "{OrderNumber}");
                    Run textPart0 = getRunFromModel(item, separate[0]);
                    textPart0.FontWeight = FontWeights.Bold;
                    Run textPart1 = getRunFromModel(item, separate[1]);
                    textPart1.FontWeight = FontWeights.Bold;

                    par = new Paragraph();  // собрать тексты в абзац
                    par.Inlines.Add(textPart0);
                    par.Inlines.Add(runOrderNumber);
                    par.Inlines.Add(textPart1);
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
                }
            }
        }

        private void addTotalLine(FlowDocument doc, decimal totalPrice, string currencyName)
        {
            //            addParagraph(doc, string.Format("______________________"), 12);
            Table t = new Table();
            t.FontSize = 14;
            t.CellSpacing = 0;
            t.BorderThickness = new Thickness(0, 1, 0, 0);
            t.BorderBrush = new SolidColorBrush(Colors.Black);
            // две колонки
            t.Columns.Add(new TableColumn() { Width = new GridLength(1, GridUnitType.Star) });
            t.Columns.Add(new TableColumn() { Width = new GridLength(1, GridUnitType.Star) });
            TableRowGroup rg = new TableRowGroup();
            t.RowGroups.Add(rg);

            string totalText = AppLib.GetLangText((Dictionary<string, string>)AppLib.GetAppGlobalValue("lblTotalText"));
            Run r = new Run(totalText);
            Paragraph p = new Paragraph(r);
            p.Margin = new Thickness(0, 7, 0, 0);
            p.TextAlignment = TextAlignment.Left;
            TableCell totalTextCell = new TableCell();
            totalTextCell.Blocks.Add(p);

            string priceText = string.Format("{0:0.00} {1}", totalPrice, currencyName);
            r = new Run(priceText);
            r.FontWeight = FontWeights.Bold;
            p = new Paragraph(r);
            p.Margin = new Thickness(0, 7, 0, 0);
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
