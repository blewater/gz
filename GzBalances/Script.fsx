#I __SOURCE_DIRECTORY__

#load "Scripts/load-references-debug.fsx"
#r "System.Net.dll"
#load "Portfolio.fs"
open GzBalances.Portfolio

getStockPrices "VTI" 2
