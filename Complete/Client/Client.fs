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
  |> Remoting.withRouteBuilder Route.builder
  |> Remoting.buildProxy<BlackjackApi>

type ApiRequest =
    | NotStarted
    | Loading

type Model = {
    Game: Game option
    StartGameRequest: ApiRequest
    HitRequest: ApiRequest
    StickRequest: ApiRequest
    ServerError: exn option
}

type ApiRequestMessage<'response> =
    | BeginRequest
    | Succeeded of response: 'response
    | Failed of exn

type Message =
    | StartGame
    | StartGameResponse of InProgressGame
    | StartGameFailed of exn
    | Hit
    | HitResponse of Game
    | HitFailed of exn
    | Stick
    | StickResponse of CompletedGame
    | StickFailed of exn

let init () =
    { Game = None
      StartGameRequest = NotStarted
      HitRequest = NotStarted
      StickRequest = NotStarted
      ServerError = None }, Cmd.none

let update (message: Message) (model: Model): Model * Cmd<Message> =
    match (message, model.Game) with
    | StartGame, _ -> { model with StartGameRequest = Loading; ServerError = None; }, Cmd.OfAsync.either blackjackApi.startGame () StartGameResponse StartGameFailed
    | StartGameResponse game, _ -> { model with Game = Some (InProgress game); StartGameRequest = NotStarted; }, Cmd.none
    | StartGameFailed error, _ -> { model with StartGameRequest = NotStarted; ServerError = Some error; }, Cmd.none

    | Hit, Some (InProgress game) -> { model with HitRequest = Loading; ServerError = None; }, Cmd.OfAsync.either blackjackApi.hit game HitResponse HitFailed
    | HitResponse game, _ -> { model with Game = Some game; HitRequest = NotStarted; }, Cmd.none
    | HitFailed error, _ -> { model with HitRequest = NotStarted; ServerError = Some error; }, Cmd.none

    | Stick, Some (InProgress game) -> { model with HitRequest = Loading; ServerError = None; }, Cmd.OfAsync.either blackjackApi.stick game StickResponse StickFailed
    | StickResponse game, _ -> { model with Game = Some (Complete game); StickRequest = NotStarted; }, Cmd.none
    | StickFailed error, _ -> { model with StickRequest = NotStarted; ServerError = Some error; }, Cmd.none

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
    let errorMessage = model.ServerError
                       |> Option.map (fun _ -> div [ Class "error" ] [ str "Oops, something went wrong. Please try again" ])
                       |> Option.defaultValue (div [] [])

    match model.Game with
    | None ->
        div [] [
            h1 [] [ str "Welcome to Blackjack!" ]
            errorMessage
            button [
                OnClick (fun _ -> dispatch StartGame)
                Classes [ if model.StartGameRequest = Loading then yield "loading"; ]
            ] [ str "Play Blackjack!" ]
        ]

    | Some (InProgress game) ->
        div [] [
            str "Game in progress..."
            deck { Deck = game.Player }
            button [
                OnClick (fun _ -> dispatch Hit)
                Classes [ if model.HitRequest = Loading then yield "loading"; ]
            ] [ str "Hit" ]
            button [
                OnClick (fun _ -> dispatch Stick)
                Classes [ if model.StickRequest = Loading then yield "loading"; ]
            ] [ str "Stick" ]
        ]

    | Some (Complete game) ->
        div [] [
            str "Game over!"
            str "Your cards:"
            deck { Deck = game.Player }
            str "Dealers cards:"
            deck { Deck = game.Dealer }
            str ("The winner was: " + (game.Winner |> string))
            button [
                OnClick (fun _ -> dispatch StartGame)
                Classes [ if model.StartGameRequest = Loading then yield "loading"; ]
            ] [ str "Play again" ]
        ]


#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactBatched "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
