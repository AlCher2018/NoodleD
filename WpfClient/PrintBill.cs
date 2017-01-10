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

        public PrintBill(OrderItem order)
        {
            _dialog = new PrintDialog(); //'Used to perform printing
            _order = order;
            _langId = AppLib.AppLang;
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
            sBuf = AppLib.GetAppSetting("BillPageWidht");
            if (sBuf == null)
            {
                errMessage = "В config-файле не указан элемент BillPageWidht с шириной отчета.";
                return false;
            }
            doc.PageWidth = double.Parse(sBuf);

            // вставить изображение в заголовок
            addImageToDoc(textHeader, doc);
            // добавить форматированный заголовок
            addSectionToDoc(textHeader, doc);

            // добавить строки заказа
            string currencyName = AppLib.GetLangText((Dictionary<string,string>)AppLib.GetAppGlobalValue("CurrencyName"));
            decimal totalPrice = 0;
            string itemName;
            foreach (DishItem item in _order.Dishes)
            {
                // блюдо
                itemName = AppLib.GetLangText(item.langNames);
                string stringRow = itemName.Substring(0, itemName.Count() > 30 ? 30 : itemName.Count());
                Run run = new Run(stringRow);
                run.FontFamily = new FontFamily("Panton-Bold");
                run.FontSize = 12;
                Paragraph par = new Paragraph(run);
                par.Margin = new Thickness(0, 0, 0, 0);
                doc.Blocks.Add(par);

                // стоимость блюда
                decimal price = item.GetPrice();
                string priceString = string.Format("{0} x {1:0.00} {3} = {2:0.00} {3}", item.Count, price, item.Count * price, currencyName);
                run = new Run(priceString);
                run.FontFamily = new FontFamily("Panton-Bold");
                run.FontSize = 12;
                par = new Paragraph(run);
                par.TextAlignment = TextAlignment.Right;
                par.Margin = new Thickness(0, 0, 130, 0);
                doc.Blocks.Add(par);
                totalPrice += item.Count * price;

                // добавить ингредиенты
                if (item.SelectedIngredients != null)
                    foreach (DishAdding ingr in item.SelectedIngredients)
                    {
                        itemName = AppLib.GetLangText(ingr.langNames);
                        stringRow = "     " + itemName.Substring(0, itemName.Count() > 30 ? 30 : itemName.Count());
                        run = new Run(stringRow);
                        run.FontFamily = new FontFamily("Panton-Bold");
                        run.FontSize = 10;
                        run.FontStyle = FontStyles.Italic;
                        par = new Paragraph(run);
                        par.Margin = new Thickness(0, 0, 0, 0);
                        doc.Blocks.Add(par);
                    }
            }
            // итог
            Run run2 = new Run(string.Format("______________________"));
            run2.FontFamily = new FontFamily("Panton-Bold");
            run2.FontSize = 20;
            Paragraph par2 = new Paragraph(run2);
            par2.Margin = new Thickness(0, 0, 0, 0);
            doc.Blocks.Add(par2);
            //     price text
            string totalText = AppLib.GetLangText((Dictionary<string, string>)AppLib.GetAppGlobalValue("lblTotalText"));
            Run runTotal = new Run(string.Format("{2}:                          {0:0.00} {1}", totalPrice, currencyName, totalText.ToUpper()));
            runTotal.FontSize = 12;
            runTotal.FontFamily = new System.Windows.Media.FontFamily("Panton-Bold");
            var parTotal = new Paragraph(runTotal);
            parTotal.Margin = new Thickness(0, 0, 0, 0);
            doc.Blocks.Add(parTotal);

            // добавить форматированный "подвал"
            addSectionToDoc(textFooter, doc);
            // вставить изображение в "подвал"
            addImageToDoc(textFooter, doc);

            _dialog.PageRange = new PageRange(1, 1);
            string printer = AppLib.GetAppSetting("PrinterName");
            _dialog.PrintQueue = new PrintQueue(new PrintServer(), printer);
            _dialog.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, "bill");

            return retVal;
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
