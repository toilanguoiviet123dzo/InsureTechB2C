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

        public static DialogOptions GetMyFormOptions(MaxWidth size = MaxWidth.Large)
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

        public static DialogOptions GetShowImageOptions(MaxWidth size = MaxWidth.Large)
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

        public static DialogOptions GetEditFormOptions(MaxWidth size = MaxWidth.Small)
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

        

        public static DialogOptions GetEditorOptions(MaxWidth size = MaxWidth.Large)
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

        public static DialogOptions GetMessageBoxOptions(MaxWidth size = MaxWidth.ExtraSmall)
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
