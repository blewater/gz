﻿namespace Everymatrix

module ExcelDownloader =

    open canopy
    
    let executingDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
    configuration.chromeDir <- executingDir

    start chrome

    let goToFirstResultFor (search:string) = 
        url "https://admin3.gammatrix.com/Admin/Login.aspx"
        "#rtbTop_text" << "admin"
        "#rtbMid_text" << "MoneyLine8!"
        "#rtbBottom_text" << "3DFEC757D808494"
        click "#btnLogin" 
        click "href[class=rmLinkrmRootLink]" 
        click (first "div.srg a")