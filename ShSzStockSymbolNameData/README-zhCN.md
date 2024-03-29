# [ShSzStockHelper-Windows](../../..)/ShSzStockSymbolNameData

[English](./README.md) | **简体中文**

这个文件夹包含了一个 Jupyter Notebook 文件，其中用 Python 代码实现了查询沪深股票代码和相应的股票名称，并将它们保存在一个 JSON 格式的文件中。这个数据文件是应用程序资源的一部分，它存储的数据经处理后用于沪深股票代码输入框的自动补全功能。

## 注意

1. IPYNB 格式的文件可以使用 Visual Studio Code（需版本支持）打开并编辑。截至2021年10月7日，使用此代码编辑器（版本：1.61.0）开发表现良好。您也可以使用 Anaconda 中的 Jupyter Notebook。此外，我想感谢 [TuShare Pro](https://tushare.pro/) 提供的宝贵数据。开发用到的主要的包参见下面的表格。

    | 名称 | 版本 |
    | :--: | :--: |
    | python | 3.8.11 |
    | tushare | 1.2.67 |
    | pandas | 1.3.3 |
