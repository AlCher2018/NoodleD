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
using System.Xml.Serialization;

namespace WpfClient
{
    public class PrintBill
    {
        PrintDialog _dialog;
        private OrderItem _order;
        string _langId;
        TakeOrderEnum _takeMode;

        public PrintBill(OrderItem order, TakeOrderEnum takeMode)
        {
            _dialog = new PrintDialog(); //'Used to perform printing
            _order = order;
            _langId = AppLib.AppLang;
            _takeMode = takeMode;
        }

        public bool CreateBill(out string errMessage)
        {
            bool retVal = true;
            errMessage = null;

            string sBuf = null;

            // создать номер счета ДЛЯ печати случайным образом
            Random rndGen = new Random();
            _order.OrderNumberForPrint = rndGen.Next(256, 1024);
            _order.OrderDate = DateTime.Now;

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
            // ширина из config-файла
            doc.PageWidth = (int)AppLib.GetAppGlobalValue("BillPageWidht", 0);
            if (doc.PageWidth == 0)
            {
                errMessage = "В config-файле не указан элемент BillPageWidht с шириной отчета.";
                return false;
            }

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
                addParagraph(doc, langText,20,FontWeights.Bold,FontStyles.Normal,new Thickness(0,20,0,10),TextAlignment.Center);
            }
            // добавить форматированный заголовок
            addSectionToDoc(textHeader, doc);

            // добавить строки заказа
            string currencyName = AppLib.GetLangText((Dictionary<string,string>)AppLib.GetAppGlobalValue("CurrencyName"));
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
                    stringRow = "     ";
                    foreach (DishAdding ingr in item.SelectedIngredients)
                    {
                        itemName = AppLib.GetLangText(ingr.langNames);
                        stringRow += itemName;
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
            string devId = (string)AppLib.GetAppGlobalValue("ssdID");
            //BarcodeLib.Barcode b = new BarcodeLib.Barcode("", BarcodeLib.TYPE.UPCA);

            _dialog.PageRange = new PageRange(1, 1);
            string printer = AppLib.GetAppSetting("PrinterName");
            _dialog.PrintQueue = new PrintQueue(new PrintServer(), printer);
            _dialog.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, "bill");

            return retVal;
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
