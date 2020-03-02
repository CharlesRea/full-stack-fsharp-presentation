module Client

open Shared
open Elmish
open Fable.Remoting.Client
open Fable.React
open Fable.React.Props
open Utils
open Fable.FontAwesome
open Fable.Core.JsInterop
open Zanaptak.TypedCssClasses

importAll "@fortawesome/fontawesome-free/css/all.css"

let blackjackApi =
  Remoting.createApi()
  |> Remoting.withRouteBuilder apiRouteBuilder
  |> Remoting.buildProxy<BlackjackApi>

let dealCard = blackjackApi.dealCard

let startGame (): Async<InProgressGame> =
    async {
        let! cards = [ dealCard (); dealCard () ] |> Async.Parallel
        return { Player = cards |> Seq.toList |> makeDeck }
    }

let hit (game: InProgressGame): Async<Game> =
    async {
        let! newCard = dealCard ()
        let newCards = newCard :: game.Player.Cards
        return match deckScore newCards with
                    | value when value > 21 -> Complete { Player = makeDeck newCards; Dealer = None; Winner = Dealer; }
                    | _ -> InProgress { Player = makeDeck newCards; }
    }

let getDealerCards () =
    let rec getCards (cards: Card list) =
        async {
            let! newCard = dealCard ()
            let newCards = newCard :: cards
            match deckScore newCards with
            | value when value > 16 -> return newCards
            | _ -> return! getCards newCards
        }

    getCards []

let stick (game: InProgressGame): Async<CompletedGame> =
    async {
        let! dealerCards = getDealerCards ()
        let playerValue = game.Player.Score
        let winner = match deckScore dealerCards with
                     | value when value > 21 -> Player
                     | value when value > playerValue -> Dealer
                     | value when value = playerValue -> Tie
                     | _ -> Player

        return { Player = game.Player; Dealer = Some (makeDeck dealerCards); Winner = winner }
    }

type ApiRequest =
    | NotStarted
    | Loading
    | Error of exn

type Model = {
    Game: Game option
    Request: ApiRequest
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
      Request = NotStarted }, Cmd.none

let update (message: Message) (model: Model): Model * Cmd<Message> =
    match (message, model.Game) with
    | StartGame, _ -> { model with Request = Loading }, Cmd.OfAsync.either startGame () StartGameSucceeded RequestFailed
    | StartGameSucceeded game, _ -> { model with Game = Some (InProgress game); Request = NotStarted; }, Cmd.none

    | Hit, Some (InProgress game) -> { model with Request = Loading; }, Cmd.OfAsync.either hit game HitSucceeded RequestFailed
    | HitSucceeded game, _ -> { model with Game = Some game; Request = NotStarted; }, Cmd.none

    | Stick, Some (InProgress game) -> { model with Request = Loading; }, Cmd.OfAsync.either stick game StickSucceeded RequestFailed
    | StickSucceeded game, _ -> { model with Game = Some (Complete game); Request = NotStarted; }, Cmd.none

    | RequestFailed error, _ -> { model with Request = Error error; }, Cmd.none

    | _ -> model, Cmd.none

type css = CssClasses<"styles.css", Naming.PascalCase>

type CardProps = { Card: Card }
let card = elmishView "Card" (fun ({ Card = (rank, suit) }: CardProps) ->
    let (suitIcon, suitClass) =
        match suit with
        | Spades -> Fa.Solid.UtensilSpoon, css.Spades
        | Hearts -> Fa.Solid.Heart, css.Hearts
        | Diamonds -> Fa.Solid.Gem, css.Diamonds
        | Clubs -> Fa.Solid.Users, css.Clubs

    let rank =
        match rank with
        | Ace -> "A"
        | Rank.Value value -> value |> string
        | Jack -> "J"
        | Queen -> "Q"
        | King -> "K"

    div [ Classes [ css.CardContainer; suitClass ] ] [
        Fa.i [ suitIcon ] [ ]
        span [ Class css.CardRank ] [ str rank ]
    ]
)

type DeckProps = { Deck: Deck }
let deck = elmishView "Deck" (fun ({ Deck = deck }: DeckProps) ->
    div [ Class css.Deck ] [
        div [ Class css.Cards ] [
            deck.Cards |> List.map (fun c -> card { Card = c }) |> ofList
        ]
        div [ Class css.Score ] [
            str ("Score: " + string deck.Score)
        ]
    ]
)

let view (model: Model) (dispatch: Message -> unit) =
    div [ ] [
        let errorMessage =
            match model.Request with
            | Error error -> div [ Class css.Error ] [ str "Oops, something went wrong. Please try again" ]
            | _ -> div [] []

        let loadingClass =
            match model.Request with
            | Loading -> css.Loading
            | _ -> ""
        match model.Game with
        | None ->
            div [ Class css.Layout ] [
                errorMessage
                button [
                    OnClick (fun _ -> dispatch StartGame)
                    Classes [ loadingClass ]
                ] [ str "Let's play blackjack!" ]
            ]

        | Some (InProgress game) ->
            div [ Class css.Layout ] [
                deck { Deck = game.Player }
                div [] [
                    str "What do you want to do?"
                ]
                div [ Class css.ButtonGroup ] [
                    button [
                        OnClick (fun _ -> dispatch Hit)
                        Classes [ loadingClass ]
                    ] [ str "Hit" ]
                    button [
                        OnClick (fun _ -> dispatch Stick)
                        Classes [ loadingClass ]
                    ] [ str "Stick" ]
                ]
            ]

        | Some (Complete game) ->
            div [ Class css.Layout ] [
                div [] [
                    match game.Winner with
                    | Player -> str "You won!"
                    | Dealer -> str "You lost!"
                    | Tie -> str "It's a tie!"
                ]

                div [] [ str "Your cards:" ]
                deck { Deck = game.Player }

                match game.Dealer with
                | Some dealer ->
                    div [] [ str "Dealers cards:" ]
                    deck { Deck = dealer }
                | None -> div [] []

                button [
                    OnClick (fun _ -> dispatch StartGame)
                    Classes [ loadingClass ]
                ] [ str "Play again" ]
            ]
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
