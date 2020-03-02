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

let dealCard = blackjackApi.dealCard

let startGame (): Async<InProgressGame> =
    async {
        let! cards = [ dealCard (); dealCard () ] |> Async.Parallel
        return { Player = cards |> Seq.toList }
    }

let hit (game: InProgressGame): Async<Game> =
    async {
        let! newCard = dealCard ()
        let newCards = newCard :: game.Player
        return match cardsValue newCards with
                    | value when value > 21 -> Complete { Player = newCards; Dealer = []; Winner = Dealer; }
                    | _ -> InProgress { Player = newCards; }
    }

let getDealerCards () =
    let rec getCards (cards: Card list) =
        async {
            let! newCard = dealCard ()
            let newCards = newCard :: cards
            match cardsValue newCards with
            | value when value > 16 -> return newCards
            | _ -> return! getCards newCards
        }

    getCards []

let stick (game: InProgressGame): Async<CompletedGame> =
    async {
        let! dealerCards = getDealerCards ()
        let playerValue = cardsValue game.Player
        let winner = match cardsValue dealerCards with
                     | value when value > 21 -> Player
                     | value when value > playerValue -> Dealer
                     | value when value = playerValue -> Tie
                     | _ -> Player

        return { Player = game.Player; Dealer = dealerCards; Winner = winner }
    }

type ApiRequest =
    | NotStarted
    | Loading
    | Error of exn

type Model = {
    Game: Game option
    DealCard: ApiRequest
}

type Message =
    | StartGame
    | StartGameSucceeded of InProgressGame
    | Hit
    | HitSucceeded of Game
    | Stick
    | StickSucceeded of CompletedGame
    | RequestFailed of exn

let init () =
    { Game = None
      DealCard = NotStarted }, Cmd.none

let update (message: Message) (model: Model): Model * Cmd<Message> =
    match (message, model.Game) with
    | StartGame, _ -> { model with DealCard = Loading }, Cmd.OfAsync.either startGame () StartGameSucceeded RequestFailed
    | StartGameSucceeded game, _ -> { model with Game = Some (InProgress game); DealCard = NotStarted; }, Cmd.none

    | Hit, Some (InProgress game) -> { model with DealCard = Loading; }, Cmd.OfAsync.either hit game HitSucceeded RequestFailed
    | HitSucceeded game, _ -> { model with Game = Some game; DealCard = NotStarted; }, Cmd.none

    | Stick, Some (InProgress game) -> { model with DealCard = Loading; }, Cmd.OfAsync.either stick game StickSucceeded RequestFailed
    | StickSucceeded game, _ -> { model with Game = Some (Complete game); DealCard = NotStarted; }, Cmd.none

    | RequestFailed error, _ -> { model with DealCard = Error error; }, Cmd.none

    | _ -> model, Cmd.none

type CardProps = { Card: Card }
let card = elmishView "Card" (fun ({ Card = (rank, suit) }: CardProps) ->
    let (suitIcon, suitClass) =
        match suit with
        | Spades -> Fa.Solid.UtensilSpoon, "spades"
        | Hearts -> Fa.Solid.Heart, "hearts"
        | Diamonds -> Fa.Solid.Gem, "diamonds"
        | Clubs -> Fa.Solid.Users, "clubs"

    let rank =
        match rank with
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
    let errorMessage =
        match model.DealCard with
        | Error error -> div [ Class "error" ] [ str "Oops, something went wrong. Please try again" ]
        | _ -> div [] []

    let loadingClass =
        match model.DealCard with
        | Loading -> "loading"
        | _ -> ""

    match model.Game with
    | None ->
        div [] [
            h1 [] [ str "Welcome to Blackjack!" ]
            errorMessage
            button [
                OnClick (fun _ -> dispatch StartGame)
                Classes [ loadingClass ]
            ] [ str "Play Blackjack!" ]
        ]

    | Some (InProgress game) ->
        div [] [
            str "Game in progress..."
            deck { Deck = game.Player }
            button [
                OnClick (fun _ -> dispatch Hit)
                Classes [ loadingClass ]
            ] [ str "Hit" ]
            button [
                OnClick (fun _ -> dispatch Stick)
                Classes [ loadingClass ]
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
                Classes [ loadingClass ]
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
