# [ShSzStockHelper-Windows](https://github.com/ArvinZJC/ShSzStockHelper-Windows)/ShSzStockSymbolNameData

[English](https://github.com/ArvinZJC/ShSzStockHelper-Windows/blob/master/ShSzStockSymbolNameData/README.md) | **简体中文**

这个文件夹包含了一个Jupyter Notebook文件，其中用Python代码实现了查询沪深股票代码和相应的股票名称，并将它们保存在一个JSON格式的文件中。这个数据文件是应用程序资源的一部分，它存储的数据经处理后用于沪深股票代码输入框的自动补全功能。

## 注意

1. 截至2020年12月16日，使用Anaconda中的Jupyter Notebook 6.1.4开发表现良好。此文件夹的IPYNB格式的文件也可以使用Visual Studio Code（需版本支持）配合Anaconda打开并编辑。此外，我想感谢[TuShare Pro](https://tushare.pro/)提供的宝贵数据。开发用到的主要的包参见下面的表格。

    | 名称 | 版本 |
    | :-- | :--: |
    | python | 3.8.5 |
    | tushare | 1.2.62 |
    | pandas | 1.1.3 |
