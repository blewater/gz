﻿module BonusReq

open System

(*
 var newBonusReq = new BonusReq()
            {
                AdminEmailRecipients = adminsArr,
                Amount = user.NetProceeds,
                Fees = user.Fees,
                GmUserId = user.GmUserId,
                InvBalIds = soldVintageDtos.Select(v => v.InvBalanceId).ToArray(),
                UserEmail = user.Email,
                UserFirstName = user.FirstName
            };
            *)


type BonusReqType = {
    AdminEmailRecipients : string[];
    Currency : string;
    Amount : decimal;
    Fees : decimal;
    GmUserId : int;
    InvBalIds : int[];
    UserEmail : string;
    UserFirstName : string;
    YearMonthSold : string;
    ProcessedCnt : int;
    CreatedOn: DateTime;
    LastProcessedTime: DateTime;
}

let incProcessCnt(bonusReq : BonusReqType) : BonusReqType=
    let newProcessedCnt = bonusReq.ProcessedCnt + 1
    let timestamp = DateTime.UtcNow
    { bonusReq with ProcessedCnt = newProcessedCnt; LastProcessedTime = timestamp }

let testBonusReq() : BonusReqType = {
    AdminEmailRecipients = [|"mario@greenzorro.com"|];
    Amount = 0.10m;
    Currency = "EUR";
    Fees = 0.025m;
    GmUserId = 4300962; // ladderman
    InvBalIds = [|588; 2783|];
    UserEmail = "salem8@gmail.com";
    UserFirstName = "Mario";
    YearMonthSold = "201802";
    ProcessedCnt = 0;
    CreatedOn = DateTime.UtcNow;
    LastProcessedTime = DateTime.UtcNow;
}
