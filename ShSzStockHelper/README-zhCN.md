# [ShSzStockHelper-Windows](https://github.com/ArvinZJC/ShSzStockHelper-Windows)/ShSzStockHelper

[English](https://github.com/ArvinZJC/ShSzStockHelper-Windows/blob/master/ShSzStockHelper/README.md) | **简体中文**

这是Visual Studio解决方案的文件夹，里面包含了应用程序运行的代码和资源。应用程序的主要功能如下：

1. 可通过沪深股票代码、开始日期和结束日期查询时间段内的成交价和成交量。
2. 在查询到的数据表上，可排序和筛选。
3. 查询到的数据表可导出到XLS或XLSX格式的Excel文件中，还可打印（打印预览窗口提供必要的打印设置）。
4. 用户可根据自己的喜好设置主题、字体、功能等。

## 用户界面示例

示例1:

![UI1.jpg](./Images_README/UI1.jpg)

示例2:

![UI2.jpg](./Images_README/UI2.jpg)

示例3:

![UI3.jpg](./Images_README/UI3.jpg)

示例4:

![UI4.jpg](./Images_README/UI4.jpg)

## 注意

1. 截至2020年10月15日，使用Visual Studio 2019和.NET Core 3.1开发表现良好。此外，我要特别感谢Syncfusion提供的[强大的WPF的UI组件库](https://www.syncfusion.com/wpf-ui-controls)，这节省了许多开发时间。
2. 图标资源（除应用程序图标）均来自[Material Design图标库](https://material.io/resources/icons/?style=baseline)和[Flaticon](https://www.flaticon.com/)。
3. 若需运行此解决方案，[项目配置文件](https://github.com/ArvinZJC/ShSzStockHelper-Windows/blob/master/ShSzStockHelper/ShSzStockHelper.csproj)中的Syncfusion程序集的路径可能需要根据实际安装情况调整。
