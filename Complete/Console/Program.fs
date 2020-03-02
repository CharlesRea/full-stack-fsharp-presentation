open Microsoft.FSharp.Reflection
open System

// Reminder of the rules:
// The objective of the game is to get a hand with a score as high as possible, while remaining below a score of 21
//
// - Ace = 11 points.
// - Jack Queen King = 10 points
// - Number = x points
//
// The player is dealt 2 cards initially.
// They can "hit" - receive a new card.
// Or "stick" - keep their hand, receive no new cards. Then the dealer has their go, and the winner is determined.

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

let random = Random()

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

let startGame (): InProgressGame =
    let cards = [ dealCard (); dealCard () ]
    { Player = makeDeck cards }

let hit (game: InProgressGame): Game =
    let newCards = dealCard () :: game.Player.Cards
    match deckScore newCards with
    | value when value > 21 -> Complete { Player = makeDeck newCards; Dealer = None;  Winner = Dealer; }
    | _ -> InProgress { Player = makeDeck newCards; }

let getDealerCards () =
    let rec getCards (cards: Card list) =
        let newCards = dealCard () :: cards
        match deckScore newCards with
        | value when value > 16 -> newCards
        | _ -> getCards newCards

    getCards []

let stick (game: InProgressGame): CompletedGame =
    let dealerCards = getDealerCards ()
    let playerValue = game.Player.Score
    let winner = match deckScore dealerCards with
                 | value when value > 21 -> Player
                 | value when value > playerValue -> Dealer
                 | value when value = playerValue -> Tie
                 | _ -> Player

    { Player = game.Player; Dealer = Some (makeDeck dealerCards); Winner = winner }

let game = startGame ()
printf "Initial deck:\r\n %A\r\n\r\n" game

match hit game with
    | InProgress game ->
        let finishedGame = stick game
        printf "You stuck, final result:\r\n %A" finishedGame
    | Complete game ->
        printf "You went over 21! Your final deck was %A" game.Player