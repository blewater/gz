//http://theburningmonk.com/2012/01/f-retry-workflow/
namespace GzDb
module AttemptBuilder =
    #if LINQPAD // doesn't actually work right in linqpad, but conveys intention, hopefully
    let dumpt (title:string) x = x.Dump(title); x
    #else
    let dumpt (title:string) x = printfn "%s:%A" title x; x    
    #endif

//    type AttemptResult<'a> = {Result: 'a option; AttemptsMade: int}
//    type Attempt<'a> = (unit -> 'a AttemptResult)
    type Attempt<'a> = (unit -> 'a option)
    let succeed x = fun () -> Some x
    let fail = fun () -> None
    let runAttempt (a:Attempt<'a>) = a()
    type Attempt = 
        static member Fail() = fail
        static member Succeed x = succeed x
        static member Run (a: Attempt<'a>) = runAttempt a
        
    let rec bind p rest i max = 
        try 
            match runAttempt p with
            |Some r -> (rest r)
            |None -> fail
        with 
        | _ when i < max -> bind p rest (i+1) max
        | _ -> fail
        
    let delay f = fun () -> runAttempt (f())
    type AttemptBuilder (maxRetry) =
        member __.Return x = succeed x
        member __.ReturnFrom (x:Attempt<'a>) = x
        member __.Bind (p,rest) = bind p rest 1 maxRetry
        member __.Delay f = delay f
        member __.Zero () = fail
    let attempt = AttemptBuilder(1)
    let retry max = AttemptBuilder(max)
    
    
open AttemptBuilder

module AttemptTests = 
    let expectedAttempt: Attempt<int> = attempt { printfn "throwing exception"; failwith "oops"}
    let ``no retry`` () = runAttempt <| attempt {
        let! a = expectedAttempt
        return a
    }
    
    let ``retry three times makes three attempts`` () = Attempt.Run <| retry 3 {
        let! a = expectedAttempt
        return a
    }
    let successTest () = Attempt.Run <| attempt { return 42 }
    let failIfBig n = attempt {
        printfn "fail if n > 1000"
        if n > 1000 then return! Attempt.Fail() else return n
    }
    let failureTest() = Attempt.Run <| retry 3 {
        let! n = failIfBig(1001)
        return n
    }
    // print something before each test, since the way they were setup before, some of the tests were firing before called (no parens at the end of test function)
    let runTest i f = 
        printfn "Starting test%i" i
        f()
        |> dumpt (sprintf "test%i result" i)
        |> ignore