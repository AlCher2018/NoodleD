using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppActionNS
{

    // 38 событий
    public enum AppActionsEnum
    {
        // Главное окно
        MainWindowOpen, MainWindowClose, CreateNewOrder,

        // события панели управления в Главном окне
        SelectLang, SelectDishCategory, ButtonPromocode, ButtonMakeOrder,
        
        // события панели блюда
        DishDescrShow, DishDescrHide, DishGarnishSelect, DishGarnishDeselect, AddDishToOrder, ButtonDishWithIngredients,

        // события Всплывашки (блюда с ингредиентами)
        DishPopupOpen, DishPopupClose,
        DishPopupIngrSelect, DishPopupIngrDeselect, DishPopupRecommendSelect, DishPopupRecommendDeselect,
        DishPopupAddButton,

        // события окна промокода
        PromocodeWinOpen, PromocodeWinClose, PromocodeInputValue,

        // события окна Корзины
        CartWinOpen, CartWinClose, ButtonPrintOrder,
        //     в панели блюда
        DishPortionAdd, DishPortionDel, ButtonDishRemove, ButtonDishRemoveResult,
        ButtonIngredientRemove, ButtonIngredientRemoveResult,

        // события окна TakeOrder
        TakeOrderWinOpen, TakeOrderWinClose,

        // события Заказа
        OrderSaveToDBResult, OrderPrintResult,

        // события Ожидашки
        IdleWindowOpen, IdleWindowClose
    }
}
