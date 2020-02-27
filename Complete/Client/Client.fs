module Client

open Browser.Dom
open Shared
open Elmish

type Cards = {
    Player: Card list
    Dealer: Card list
}

type Game =
    | NotStarted
    | InProgress of Cards
    | Complete of cards: Cards * winner: string

type Remote<'response> =
    | NotStarted
    | InProgress
    | Error of exn
    | Body of 'response

type Model = {
    Game: Game
    FetchCard: Remote<Card>
}

type Message =
    | GameStarted
    | Hit
    | FetchCardSucceeded
    | FetchCardFailed
    | Stick
    | GameComplete of winner: string

let update (message: Message) (model: Model): Model * Cmd<Message> =
    match message with
    | GameStarted -> { model with Game = Game.InProgress ({ Plaer =  }) }, Cmd.none