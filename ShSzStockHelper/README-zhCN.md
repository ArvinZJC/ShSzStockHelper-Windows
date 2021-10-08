# [ShSzStockHelper-Windows](../../..)/ShSzStockHelper

[English](./README.md) | **简体中文**

这是 Visual Studio 解决方案的文件夹，里面包含了应用程序运行的代码和资源。应用程序的主要功能如下：

1. 可通过沪深股票代码、开始日期和结束日期查询时间段内的成交价和成交量。（数据查询通过应用网络爬虫实现。）
2. 在查询到的数据表上，可排序和筛选。
3. 查询到的数据表可导出到 XLS 或 XLSX 格式的 Excel 文件中，还可打印（打印预览窗口提供必要的打印设置）。
4. 用户可根据自己的喜好设置主题、字体、功能等。

## 注意

1. 截至2021年6月20日，使用 Visual Studio 2019（版本：16.10.2）和 Windows 呈现基础（WPF，.NET Core 3.1）开发表现良好。此外，我要特别感谢 Syncfusion 提供的[强大的 WPF 的 UI 组件库](https://www.syncfusion.com/wpf-ui-controls)，这节省了许多开发时间。
2. 应用用到的图标资源主要来自 [Material Design 图标库](https://material.io/resources/icons/?style=baseline)和 [Flaticon](https://www.flaticon.com/)。
3. 应用用到的主要的NuGet包参见下面的表格。

    | 名称 | 版本 |
    | :-- | :--: |
    | [HtmlAgilityPack](https://html-agility-pack.net/) | 1.11.34 |
    | [Newtonsoft.Json](https://www.newtonsoft.com/json) | 13.0.1 |
    | [PortableSettingsProvider](https://github.com/Bluegrams/SettingsProviders) | 0.2.4 |
    | Syncfusion.DataGridExcelExport.Wpf | 19.1.0.69 |
    | Syncfusion.SfBusyIndicator.WPF | 19.1.0.69 |
    | Syncfusion.SfGrid.WPF | 19.1.0.69 |
    | Syncfusion.SfInput.WPF | 19.1.0.69 |
    | Syncfusion.SfTreeNavigator.WPF | 19.1.0.69 |
    | Syncfusion.Themes.MaterialDark.WPF | 19.1.0.69 |
    | Syncfusion.Themes.MaterialLight.WPF | 19.1.0.69 |
    | Syncfusion.Tools.WPF | 19.1.0.69 |

## 用户界面示例

示例1:

![UI1.png](./Images_README/UI1.png)

示例2:

![UI2.png](./Images_README/UI2.png)

示例3:

![UI3.png](./Images_README/UI3.png)

示例4:

![UI4.png](./Images_README/UI4.png)

示例5:

![UI5.png](./Images_README/UI5.png)

示例6:

![UI6.png](./Images_README/UI6.png)
