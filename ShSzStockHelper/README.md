# [ShSzStockHelper-Windows](https://github.com/ArvinZJC/ShSzStockHelper-Windows)/ShSzStockHelper

**English** | [简体中文](https://github.com/ArvinZJC/ShSzStockHelper-Windows/blob/master/ShSzStockHelper/README-zhCN.md)

This is the Visual Studio solution folder of the application. It contains code and resources for the application to run. The major features of the application are listed as follows.

1. Enable searching strike prices and volumes by the symbol of an SH/SZ stock, the start date, and the end date.
2. Allow sorting and filtering on the columns of the table containing data of strike prices and volumes.
3. The data retrieved can be exported to an Excel file (.xls/.xlsx), and it is also available to be printed. A Print Preview window with printing settings will be displayed.
4. The preferences for themes, fonts, functionality, etc. can be changed by the user.

## Partial UI

UI 1:

![UI1.jpg](./Images_README/UI1.jpg)

UI 2:

![UI2.jpg](./Images_README/UI2.jpg)

UI 3:

![UI3.jpg](./Images_README/UI3.jpg)

UI 4:

![UI4.jpg](./Images_README/UI4.jpg)

## ATTENTION

1. By 15 October 2020, everything looks good with Visual Studio 2019 + WPF with .NET Core 3.1. Additionally, I would like to thankfully acknowledge Syncfusion for [its powerful WPF UI controls and libraries](https://www.syncfusion.com/wpf-ui-controls), which save much development time.
2. Icon images except the icon of the application are from [Material Design Icons](https://material.io/resources/icons/?style=baseline) and [Flaticon](https://www.flaticon.com/).
3. To run the solution, the paths of Syncfusion assemblies in the [project configuration file](https://github.com/ArvinZJC/ShSzStockHelper-Windows/blob/master/ShSzStockHelper/ShSzStockHelper.csproj) may need adjustment.
