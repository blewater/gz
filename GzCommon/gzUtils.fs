[<AutoOpen>]
module FsUtils

/// Function forword Pipe Debugging Aid
//let getProjects searchDirs =
//	searchDirs 
//	|> breakpoint (Seq.map projectPathsIn) 
//	|> Seq. collect (fun p -> p)
//	|      > Seq. map xdoc 
let breakpoint fn value =
    let result = fn value 
    result //<--- Breakpoint

// Credit: http://onoffswitch.net/debugging-piped-sequences-f/
//let seqDebug =
//        [0..1000]
//                |> List.map (fun i -> i + 1)
//                |> ~~ Console.WriteLine
//                |> List.filter (fun i -> i < 3)
//                |> ~~ (printfn "%A")
//                |> List.head
//                |> ~~ Console.WriteLine
let (~~) (func:'a-> unit) (arg:'a) = (func arg) |> fun () -> arg

//let seqDebug =
//        [0..1000]
//                |> List.map (fun i -> i + 1)
//                |> identity
//                |> List.filter (fun i -> i < 5)
//                |> List.head
let identity item = item

// Effiecient F# null checking
let objIsNotNull(x : 't when 't : null) = not <| System.Object.ReferenceEquals(x, null)
