using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp.Client.Common
{
    public static class MyOptions
    {
        public static DialogOptions ShowAppbarOptions(MaxWidth size = MaxWidth.Small)
        {
            var options = new DialogOptions()
            {
                MaxWidth = size,
                Position = DialogPosition.Center,
                CloseOnEscapeKey = false,
                DisableBackdropClick = true,
                CloseButton = false,
                FullWidth = false
            };
            return options;
        }

        public static DialogOptions ShowPopupEditOptions(MaxWidth size = MaxWidth.Small)
        {
            var options = new DialogOptions()
            {
                MaxWidth = size,
                Position = DialogPosition.Center,
                CloseOnEscapeKey = true,
                DisableBackdropClick = true,
                CloseButton = false,
                FullWidth = false
            };
            return options;
        }

        public static DialogOptions ShowPopupOptions(MaxWidth size = MaxWidth.Small)
        {
            var options = new DialogOptions()
            {
                MaxWidth = size,
                Position = DialogPosition.Center,
                CloseOnEscapeKey = true,
                DisableBackdropClick = true,
                CloseButton = false,
                FullWidth = false
            };
            return options;
        }

        public static DialogOptions ShowImageOptions(MaxWidth size = MaxWidth.Large)
        {
            var options = new DialogOptions()
            {
                MaxWidth = size,
                Position = DialogPosition.Center,
                CloseOnEscapeKey = true,
                DisableBackdropClick = false,
                CloseButton = true,
                FullWidth = false
            };
            return options;
        }

        public static DialogOptions ShowMessageBoxOptions(MaxWidth size = MaxWidth.ExtraSmall)
        {
            var options = new DialogOptions()
            {
                MaxWidth = size,
                Position = DialogPosition.Center,
                CloseOnEscapeKey = true,
                DisableBackdropClick = true,
                CloseButton = false,
                FullWidth = false
            };
            return options;
        }
    }
}
