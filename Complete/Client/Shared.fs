module Shared

type Suit =
    | Hearts
    | Clubs
    | Diamonds
    | Spades

type Rank =
    | Value of int
    | Ace
    | King
    | Queen
    | Jack

type Card = Rank * Suit

type Winner =
    | Player
    | Dealer
    | Tie

type InProgressGame = { Player: Card list }
type CompletedGame = { Player: Card list; Dealer: Card list; Winner: Winner }

type Game =
    | InProgress of InProgressGame
    | Complete of CompletedGame

let cardValue ((rank, _): Card) =
    match rank with
    | Value i -> i
    | Ace -> 11
    | _ -> 10

let cardsValue cards =
    cards |> List.map cardValue |> List.sum

module Route =
    let builder typeName methodName =
        sprintf "/api/%s" methodName

type BlackjackApi = {
    dealCard: unit -> Async<Card>
}