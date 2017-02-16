namespace GzCommon

[<AutoOpen>]
module DateStr =
    open System

    /// Strongly-typed shortcut for Enum.TryParse(). Defaults to ignoring case. Ref http://www.extensionmethod.net/fsharp/string/string-toenum-a
    type String with
        member this.ToEnum<'a when 'a :> Enum and 'a : struct and 'a : (new: unit -> 'a)> (?ignoreCase) =
            let ok, v = Enum.TryParse<'a>(this, defaultArg ignoreCase true)
            if ok then Some v else None

    /// Converts a Nullable type into an Option. Ref http://www.extensionmethod.net/fsharp/nullable-t/nullable-asoption
    type Nullable<'T when 'T : struct and 'T :> ValueType and 'T:(new: unit -> 'T)> with
        member inline this.AsOption =
            if this.HasValue then Some(this.Value) else None

    type DateTime with
        member this.ToYyyyMm =
            this.Year.ToString("0000") + this.Month.ToString("00")
            
    type DateTime with
        member this.ToYyyyMmDd = 
            this.ToYyyyMm + this.Day.ToString("00") 

    type DateTime with
        member this.ToPrevYyyyMm = 
            let prev = this.AddMonths(-1)
            prev.ToYyyyMm

    type String with
        member this.ToDateWithDay = 
            match DateTime.TryParseExact(this, "yyyyMMdd", null, Globalization.DateTimeStyles.None) with
            | true, date -> date
            | false, _ -> invalidArg "Cannot parse a Date in this string" (sprintf "this string %s." this)

    type String with
        member this.ToPrevYyyyMm = 
            match DateTime.TryParseExact(this + "1", "yyyyMMd", null, Globalization.DateTimeStyles.None) with
            | true, date -> date.AddMonths(-1).ToYyyyMm
            | false, _ -> invalidArg "Cannot parse a Date in this string" (sprintf "this string %s." this)

    type String with
        member this.ToNextMonth1st = 
            match DateTime.TryParseExact(this + "1", "yyyyMMd", null, Globalization.DateTimeStyles.None) with
            | true, date -> date.AddMonths(1).ToYyyyMmDd
            | false, _ -> invalidArg "Cannot parse a Date in this string" (sprintf "this string %s." this)

    type String with
        member this.ToEndOfMonth = 
            match DateTime.TryParseExact(this + "1", "yyyyMMd", null, Globalization.DateTimeStyles.None) with
            | true, date -> date.AddMonths(1).AddDays(-1.0).ToYyyyMmDd
            | false, _ -> invalidArg "Cannot parse a Date in this string" (sprintf "this string %s." this)

    type String with
        member this.ToDateOn1st = 
            (this + "01").ToDateWithDay
