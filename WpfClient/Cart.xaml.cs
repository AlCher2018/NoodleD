using AppModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for Cart.xaml
    /// </summary>
    public partial class Cart : Window
    {
        // dragging
        Point? lastDragPoint;
        protected DateTime _dateTime;
        bool _isMoved;


        public Cart()
        {
            InitializeComponent();

            OrderItem currentOrder = (OrderItem)AppLib.GetAppGlobalValue("currentOrder");
            this.lstDishes.ItemsSource = currentOrder.Dishes;
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) closeWin();
        }

        private void txtbackToMenu_MouseUp(object sender, MouseButtonEventArgs e)
        {
            closeWin();
        }



        #region dish list behaviour
        private void lstDishes_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }

        private void initDrag(Point mousePos)
        {
            _dateTime = DateTime.Now;
            _isMoved = false;
            //make sure we still can use the scrollbars
            if (mousePos.X <= scrollDishes.ViewportWidth && mousePos.Y < scrollDishes.ViewportHeight)
            {
                //scrollViewer.Cursor = Cursors.SizeAll;
                lastDragPoint = mousePos;
                //Mouse.Capture(scrollViewer);
            }
        }
        private void endDrag()
        {
            //scrollViewer.Cursor = Cursors.Arrow;
            //scrollViewer.ReleaseMouseCapture();
            lastDragPoint = null;
        }
        private void doMove(Point posNow)
        {
            double dX = posNow.X - lastDragPoint.Value.X;
            double dY = posNow.Y - lastDragPoint.Value.Y;

            lastDragPoint = posNow;

            scrollDishes.ScrollToHorizontalOffset(scrollDishes.HorizontalOffset - dX);
            scrollDishes.ScrollToVerticalOffset(scrollDishes.VerticalOffset - dY);
            _isMoved = true;
        }
        private void scrollDishes_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            initDrag(e.GetPosition(scrollDishes));
        }

        private void scrollDishes_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            initDrag(e.GetTouchPoint(scrollDishes).Position);
        }

        private void scrollDishes_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            endDrag();
        }

        private void scrollDishes_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            endDrag();
        }

        private void scrollDishes_MouseMove(object sender, MouseEventArgs e)
        {
            if (lastDragPoint.HasValue && e.LeftButton == MouseButtonState.Pressed)
            {
                doMove(e.GetPosition(sender as IInputElement));
            }
        }

        private void scrollDishes_PreviewTouchMove(object sender, TouchEventArgs e)
        {
            if (lastDragPoint.HasValue)
            {
                doMove(e.GetTouchPoint((sender as IInputElement)).Position);
            }
        }

        #endregion

        private void closeWin()
        {
            this.Close();
        }

    }
}
