﻿using System.Windows;

namespace SnowyRiver.WPF.MaterialDesignInPrism.Windows;
public class MaterialDesignMetroDialogWindow : MaterialDesignMetroWindow,IDialogWindow
{
    public MaterialDesignMetroDialogWindow()
    {
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        ShowMinButton = false;
        ShowMaxRestoreButton = false;
    }

    public IDialogResult? Result { get; set; }

    protected override bool EnableActivateOnShowMeWindowMessage => true;
}
