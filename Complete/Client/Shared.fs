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

type Deck = { Cards: Card list; Score: int }

type Winner =
    | Player
    | Dealer
    | Tie

type InProgressGame = { Player: Deck }
type CompletedGame = { Player: Deck; Dealer: Deck option; Winner: Winner }

type Game =
    | InProgress of InProgressGame
    | Complete of CompletedGame

let cardScore ((rank, _): Card) =
    match rank with
    | Value i -> i
    | Ace -> 11
    | _ -> 10

let deckScore cards =
    cards |> List.map cardScore |> List.sum

let makeDeck cards =
    { Cards = cards; Score = deckScore cards }

let apiRouteBuilder typeName methodName =
    sprintf "/api/%s" methodName

type BlackjackApi = {
    dealCard: unit -> Async<Card>
}