using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppActionNS
{
    public enum AppActionsEnum
    {
        // Главное окно
        MainWindowOpen, MainWindowClose, CreateNewOrder,

        // события панели управления в Главном окне
        SelectLang, SelectDishCategory, ButtonPromocode, ButtonMakeOrder,
        
        // события панели блюда
        DishDescrShow, DishDescrHide, DishGarnishSelect, DishGarnishUnselect, AddDishToOrder, ButtonDishWithIngredients

    }
}
