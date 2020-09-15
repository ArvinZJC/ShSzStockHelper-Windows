# ShSzStockHelper for Windows

This repository contains the source code of the Windows version of the application SH/SZ Stock Helper (沪深小助手). Please note that the code is under [the GPL-3.0 License](https://github.com/ArvinZJC/ShSzStockHelper-Windows/blob/master/LICENSE). The application can mainly search strike prices and volumes for the user. Here SH represents Shanghai Stock Exchange, while SZ means Shenzhen Stock Exchange. Please note that this application is mainly designed for a specified part of Chinese users, and contents are displayed in simplified Chinese.

## Folder Instructions

### [ShSzStockHelper](https://github.com/ArvinZJC/ShSzStockHelper-Windows/tree/master/ShSzStockHelper)

This is the Visual Studio solution folder of the application. It contains code and resources for the application to run.

### [ShSzStockHelper_Setup](https://github.com/ArvinZJC/ShSzStockHelper-Windows/tree/master/ShSzStockHelper_Setup)

This is the Advanced Installer project folder of the application setup file. The files in the folder are mainly used to build the latest application setup file (.exe) with user-friendly UI.

### [ShSzStockSymbolNameData](https://github.com/ArvinZJC/ShSzStockHelper-Windows/tree/master/ShSzStockSymbolNameData)

This folder contains a Jupyter Notebook file with Python code to retrieve and store a list of symbols and corresponding names of SH/SZ stocks in a JSON file which is a part of the resources of the application.

## ATTENTION

1. The setup EXE files can be downloaded from [Releases](https://github.com/ArvinZJC/ShSzStockHelper-Windows/releases) section. From [V0.3.0](https://github.com/ArvinZJC/ShSzStockHelper-Windows/releases/tag/v0.3.0), a totally new solution to build the setup file is applied to make it more powerful (but with a smaller size) and **any version older than V0.3.0 should be manually uninstalled**.

For more information, please refer to the README file in each folder.
