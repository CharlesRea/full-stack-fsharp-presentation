module Client

open Shared
open Elmish
open Fable.Remoting.Client
open Fable.React
open Fable.React.Props
open Utils
open Fable.FontAwesome
open Fable.Core.JsInterop

importAll "@fortawesome/fontawesome-free/css/all.css"

let blackjackApi =
  Remoting.createApi()
  |> Remoting.buildProxy<BlackjackApi>

type ApiRequest =
    | NotStarted
    | Loading
    | Error of exn

type Model = {
    Game: Game option
    StartGame: ApiRequest
    Hit: ApiRequest
    Stick: ApiRequest
}

type ApiRequestMessage<'response> =
    | BeginRequest
    | Succeeded of response: 'response
    | Failed of exn

type Message =
    | StartGame of ApiRequestMessage<InProgressGame>
    | Hit of ApiRequestMessage<Game>
    | Stick of ApiRequestMessage<CompletedGame>

let init () =
    { Game = None; StartGame = NotStarted; Hit = NotStarted; Stick = NotStarted; }, Cmd.none

let update (message: Message) (model: Model): Model * Cmd<Message> =
    match (message, model.Game) with
    | StartGame BeginRequest, _ -> { model with StartGame = Loading; }, Cmd.OfAsync.either blackjackApi.startGame () (Succeeded >> StartGame) (Failed >> StartGame)
    | StartGame (Succeeded game), _ -> { model with Game = Some (InProgress game); StartGame = NotStarted; }, Cmd.none
    | StartGame (Failed error), _ -> { model with StartGame = Error error }, Cmd.none

    | Hit BeginRequest, Some (InProgress game) -> { model with Hit = Loading; }, Cmd.OfAsync.either blackjackApi.hit (game) (Succeeded >> Hit) (Failed >> Hit)
    | Hit (Succeeded game), _ -> { model with Game = Some game; Hit = NotStarted; }, Cmd.none
    | Hit (Failed error), _ -> { model with Hit = Error error }, Cmd.none

    | Stick BeginRequest, Some (InProgress game) -> { model with Stick = Loading; }, Cmd.OfAsync.either blackjackApi.stick (game) (Succeeded >> Stick) (Failed >> Stick)
    | Stick (Succeeded game), _ -> { model with Game = Some (Complete game); Stick = NotStarted; }, Cmd.none
    | Stick (Failed error), _ -> { model with Stick = Error error }, Cmd.none

    | _ -> model, Cmd.none

type CardProps = { Card: Card }
let card = elmishView "Card" (fun ({ Card = card }: CardProps) ->
    let (suitIcon, suitClass) =
        match card.Suit with
        | Spades -> Fa.Solid.UtensilSpoon, "spades"
        | Hearts -> Fa.Solid.Heart, "hearts"
        | Diamonds -> Fa.Solid.Gem, "diamonds"
        | Clubs -> Fa.Solid.Users, "clubs"

    let rank =
        match card.Rank with
        | Ace -> "A"
        | Rank.Value value -> value |> string
        | Jack -> "J"
        | Queen -> "Q"
        | King -> "K"

    div [ Class ("card-container " + suitClass) ] [
        Fa.i [ suitIcon ] [ ]
        span [ Class "card-rank" ] [ str rank ]
    ]
)

type DeckProps = { Deck: Card list }
let deck = elmishView "Deck" (fun ({ Deck = deck }: DeckProps) ->
    div [ Class "deck" ] [
        deck |> List.map (fun c -> card { Card = c }) |> ofList
    ]
)

let view (model: Model) (dispatch: Message -> unit) =
    div[] [ str "Hello world" ]
