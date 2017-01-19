namespace DbImport

[<AutoOpen>]
module DateStr =
    open System

    type String with
        /// Strongly-typed shortcut for Enum.TryParse(). Defaults to ignoring case.
        member this.ToEnum<'a when 'a :> Enum and 'a : struct and 'a : (new: unit -> 'a)> (?ignoreCase) =
            let ok, v = Enum.TryParse<'a>(this, defaultArg ignoreCase true)
            if ok then Some v else None

    type Nullable<'T when 'T : struct and 'T :> ValueType and 'T:(new: unit -> 'T)> with
        /// Converts a Nullable type into an Option.
        member inline this.AsOption =
            if this.HasValue then Some(this.Value) else None

    type DateTime with
        member this.ToYyyyMm =
            this.Year.ToString() + this.Month.ToString("00")
            
    type DateTime with
        member this.ToYyyyMmDd = 
            this.ToYyyyMm + this.Day.ToString("00") 

    type String with
        member this.ToDateWithDay = 
            match DateTime.TryParseExact(this, "yyyymmdd", null, Globalization.DateTimeStyles.None) with
            | true, date -> date
            | false, _ -> invalidArg "Cannot parse a Date in this string" (sprintf "this string %s." this)

    type String with
        member this.ToDateOn1st = 
            (this + "01").ToDateWithDay
