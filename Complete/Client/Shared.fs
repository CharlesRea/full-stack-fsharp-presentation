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

type Card =
    { Suit: Suit
      Rank: Rank }

type Winner =
    | Player
    | Dealer
    | Tie

type InProgressGame = { Player: Card list }
type CompletedGame = { Player: Card list; Dealer: Card list; Winner: Winner }

type Game =
    | InProgress of InProgressGame
    | Complete of CompletedGame

let cardValue card =
    match card.Rank with
    | Value i -> i
    | Ace -> 11
    | _ -> 10

let cardsValue cards =
    List.sumBy cardValue cards

type BlackjackApi = {
    startGame: unit -> Async<InProgressGame>
    hit: InProgressGame -> Async<Game>
    stick: InProgressGame -> Async<CompletedGame>
}