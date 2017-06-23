module ExcelUtil

    open System

    (*----Helpers *)

    /// Zero out null values in Nullables
    let ifNull0Decimal (nullableDec : decimal Nullable) : decimal Nullable =
        match nullableDec.HasValue with
        | false -> Nullable 0m
        | _ -> nullableDec

    /// From (excel's default decimal type) to Db nullable decimals
    let float2NullableDecimal (excelFloatExpr : float) : decimal Nullable =
        excelFloatExpr |> Convert.ToDecimal |> Nullable<decimal>

    /// Convert string boolean literal to bool Nullable
    let string2NullableBool (excelFloatExpr : string) : bool Nullable = 
        excelFloatExpr |> Convert.ToBoolean |> Nullable<bool> 

    type DateMask =
    | CustomRpt
    | WithdrawalRpt

    let excelObjNullableString (excelObjStr : obj) : string =
        if not <| isNull excelObjStr then
            excelObjStr.ToString()
        else
            null

    /// Convert DateTime object expression to DateTime Nullable
    let excelObj2NullableDt (dateMask : DateMask) (excelObjDt : obj) : DateTime Nullable = 
        let dateMaskStr = 
            match dateMask with
            | CustomRpt -> "yyyy-MM-dd"
            | WithdrawalRpt -> "dd/MM/yyyy HH:mm"

        let nullableDate = System.Nullable<DateTime>()
        if not <| isNull excelObjDt then
            match DateTime.TryParseExact(excelObjDt.ToString(), dateMaskStr, null, Globalization.DateTimeStyles.None) with
                | true, dtRes -> Nullable dtRes
                | false, _ -> nullableDate
        else nullableDate

    /// Cast excel User id to int guarding against for "totals" line
    let getNonNullableUserId (excelUserId : string) : int =
        match excelUserId with
        | null -> 0
        | userIdStr -> int userIdStr




