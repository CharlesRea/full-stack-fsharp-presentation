module BlackjackApi

open System
open Microsoft.FSharp.Reflection
open Shared
open Giraffe

let random = Random()
let suits = FSharpType.GetUnionCases typeof<Suit>

let randomSuit () =
    match random.Next(4) with
    | 0 -> Spades
    | 1 -> Clubs
    | 2 -> Hearts
    | 3 -> Diamonds
    | _ -> failwith "Invalid value"

let randomRank () =
    match random.Next(1, 14) with
    | 1 -> Ace
    | 11 -> Jack
    | 12 -> Queen
    | 13 -> King
    | i -> Value i

let dealCard (): Card =
    (randomRank (), randomSuit ())

let blackJackApi: BlackjackApi = {
    dealCard = fun () -> async { return dealCard () }
}
