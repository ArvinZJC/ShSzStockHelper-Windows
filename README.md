# SH/SZ Stock Helper for Windows

[![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/ArvinZJC/ShSzStockHelper-Windows?include_prereleases)](https://github.com/ArvinZJC/ShSzStockHelper-Windows/releases)
[![GitHub All Releases](https://img.shields.io/github/downloads/ArvinZJC/ShSzStockHelper-Windows/total)](https://github.com/ArvinZJC/ShSzStockHelper-Windows/releases)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/980d1c6c75754cdf9900139f5c5eb66f)](https://www.codacy.com/gh/ArvinZJC/ShSzStockHelper-Windows/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=ArvinZJC/ShSzStockHelper-Windows&amp;utm_campaign=Badge_Grade)
[![License: GPL-3.0](https://img.shields.io/badge/license-GPL--3.0-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)

**English** | [简体中文](https://github.com/ArvinZJC/ShSzStockHelper-Windows/blob/master/README-zhCN.md)

SH/SZ Stock Helper can mainly search strike prices and volumes for the users. Here SH represents Shanghai Stock Exchange, while SZ means Shenzhen Stock Exchange. Please note that this application is mainly designed for a specified part of Chinese users, and contents are displayed in simplified Chinese.

This repository contains the source code of the Windows version of the application. Please note that the code is licensed under [the GPL-3.0 License](https://github.com/ArvinZJC/ShSzStockHelper-Windows/blob/master/LICENSE).

## Folder Instructions

### [ShSzStockHelper](https://github.com/ArvinZJC/ShSzStockHelper-Windows/tree/master/ShSzStockHelper)

This is the Visual Studio solution folder of the application. It contains code and resources for the application to run.

### [ShSzStockHelper_Setup](https://github.com/ArvinZJC/ShSzStockHelper-Windows/tree/master/ShSzStockHelper_Setup)

This is the Advanced Installer project folder. The files in the folder are mainly used to build the latest application setup file (.exe) with user-friendly UI.

### [ShSzStockSymbolNameData](https://github.com/ArvinZJC/ShSzStockHelper-Windows/tree/master/ShSzStockSymbolNameData)

This folder contains a Jupyter Notebook file with Python code to retrieve and store a list of symbols and corresponding names of SH/SZ stocks in a JSON file which is a part of the resources of the application.

## ATTENTION

1. The setup EXE files can be downloaded from [Releases](https://github.com/ArvinZJC/ShSzStockHelper-Windows/releases) section. From [V0.3.0](https://github.com/ArvinZJC/ShSzStockHelper-Windows/releases/tag/v0.3.0), a totally new solution to build the setup file is applied to make it more powerful (but with a smaller size) and **any version older than V0.3.0 should be manually uninstalled**.
2. The application supports Windows 7/8/8.1/10.

For more information, please refer to the README file in each folder.
