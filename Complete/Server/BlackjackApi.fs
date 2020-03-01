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

let getCard (): Card =
    { Suit = randomSuit (); Rank = randomRank () }

let startGame (): InProgressGame =
    let cards = [ getCard (); getCard () ]
    { Player = cards }

let hit (game: InProgressGame): Game =
    let newCards = getCard () :: game.Player
    match cardsValue newCards with
    | value when value > 21 -> Complete { Player = newCards; Dealer = []; Winner = Dealer; }
    | _ -> InProgress { Player = newCards; }

let getDealerCards () =
    let rec dealCard cards =
        let newCards = getCard () :: cards
        match cardsValue newCards with
        | value when value > 16 -> newCards
        | _ -> dealCard newCards

    dealCard []

let stick (game: InProgressGame): CompletedGame =
    let dealerCards = getDealerCards ()
    let playerValue = cardsValue game.Player
    let winner = match cardsValue dealerCards with
                 | value when value > 21 -> Player
                 | value when value > playerValue -> Dealer
                 | value when value = playerValue -> Tie
                 | _ -> Player

    { Player = game.Player; Dealer = dealerCards; Winner = winner }

let blackjackHandler: HttpHandler =
    choose [
        GET >=> route "/begin" >=> warbler (fun _ -> (startGame () |> json))
        POST >=> choose [
            route "/hit" >=> bindJson<InProgressGame>(hit >> json)
            route "/stick" >=> bindJson<InProgressGame>(stick >> json)
        ]
    ]

let blackJackApi: BlackjackApi = {
    startGame = fun () -> async { return startGame () }
    hit = fun (game) -> async { return hit (game) }
    stick = fun (game) -> async { return stick (game) }
}
