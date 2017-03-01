using AppModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace WpfClient
{
    public class MainMenuDishesCanvas: Canvas
    {
        AppModel.MenuItem mItem;


        public MainMenuDishesCanvas(AppModel.MenuItem menuItem)
        {
            mItem = menuItem;

            createDishesCanvas();
        }

        private void createDishesCanvas()
        {
            //  РАЗМЕРЫ ПАНЕЛИ БЛЮД
            double dishesPanelWidth = (double)AppLib.GetAppGlobalValue("dishesPanelWidth");
            double dishPanelLeftMargin = (double)AppLib.GetAppGlobalValue("dishPanelLeftMargin");
            double dishPanelWidth = (double)AppLib.GetAppGlobalValue("dishPanelWidth");
            
            int iRowsCount = 0;
            if (mItem.Dishes.Count > 0) iRowsCount = ((mItem.Dishes.Count - 1) / 3) + 1;

            DishItem dish;
            int iRow, iCol; double leftPos, topPos;
            for (int i = 0; i < mItem.Dishes.Count; i++)
            {
                dish = mItem.Dishes[i];

                // размеры канвы с панелями блюд
                bool isExistGarnishes = (dish.Garnishes != null);
                double currentPanelHeight = (isExistGarnishes) ? (double)AppLib.GetAppGlobalValue("dishPanelHeightWithGarnish") : (double)AppLib.GetAppGlobalValue("dishPanelHeight");
                if (double.IsNaN(base.Width) || (base.Width == 0))
                {
                    base.Width = dishesPanelWidth;
                    base.Height = iRowsCount * currentPanelHeight;
                }

                // положение панели блюда
                iRow = i / 3; iCol = i % 3;
                leftPos = (dishPanelLeftMargin + iCol * dishPanelWidth);
                topPos = iRow * currentPanelHeight;

                MainMenuDishPanel dishPanel = new MainMenuDishPanel(dish, leftPos, topPos);

                base.Children.Add(dishPanel);

            }  // for dishes

            //base.Background = Brushes.Gold;
            //base.Background = new SolidColorBrush(new Color() { R = (byte)rnd.Next(0,254), G= (byte)rnd.Next(0, 254), B= (byte)rnd.Next(0, 254), A=0xFF });
        } // method

        public void ClearSelectedGarnish()
        {
            foreach (MainMenuDishPanel dishPanel in this.Children)
            {
                if ((dishPanel.HasGarnishes) && (dishPanel.SelectedGarnishIndex > -1))
                {
                    dishPanel.ClearSelectedGarnish();
                }
            }
        }

        public void HideDishesDescriptions()
        {
            foreach (MainMenuDishPanel dishPanel in this.Children)
            {
                if (dishPanel.IsDescriptionShow == true) dishPanel.HideDescription();
            }
        }


        public void ResetLang()
        {
            foreach (MainMenuDishPanel dishPanel in this.Children)
            {
                dishPanel.ResetLang();
            }

        }


    } // class
}
