#I __SOURCE_DIRECTORY__

#load "Scripts/load-references-production.fsx"

#load "Portfolio.fs"
open GzBalances.Portfolio

getStockPrices "VTI" 2
