# Console app

### Modeling cards:
```fsharp
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
```

### Modelling game:
```fsharp
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
```

### Calculating score:
```fsharp
let cardScore ((rank, _): Card) =
    match rank with
    | Value i -> i
    | Ace -> 11
    | _ -> 10

let deckScore cards =
    cards |> List.map cardScore |> List.sum

let makeDeck cards =
    { Cards = cards; Score = deckScore cards }
```

### Deal cards
```fsharp

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
```

### Start game
```fsharp

let startGame (): InProgressGame =
    let cards = [ dealCard (); dealCard () ]
    { Player = makeDeck cards }
```

### Hit
```fsharp
let hit (game: InProgressGame): Game =
    let newCards = dealCard () :: game.Player.Cards
    match deckScore newCards with
    | value when value > 21 -> Complete { Player = makeDeck newCards; Dealer = None;  Winner = Dealer; }
    | _ -> InProgress { Player = makeDeck newCards; }
```

### Stick
```fsharp
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
```

### Basic demo
```fsharp
let game = startGame ()
printf "Initial deck:\r\n %A\r\n\r\n" game

match hit game with
    | InProgress game ->
        let finishedGame = stick game
        printf "You stuck, final result:\r\n %A" finishedGame
    | Complete game ->
        printf "You went over 21! Your final deck was %A" game.Player
```


# API

### Giraffe hello world
```fsharp
module Server

open Blackjack
open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Thoth.Json.Giraffe

type GreetingResponse =
    {
        Greeting : string
    }

let greetingsHandler (name : string) =
    let greetings = sprintf "Hello %s, from Giraffe!" name
    let response = { Greeting = greetings }
    json response

let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> greetingsHandler "world"
                routef "/hello/%s" greetingsHandler
                route "/ping"   >=> text "pong"
            ]
        setStatusCode 404 >=> text "Not Found" ]

let configureApp (app : IApplicationBuilder) =
    app.UseDeveloperExceptionPage()
        .UseStaticFiles()
        .UseGiraffe webApp

let configureServices (services : IServiceCollection) =
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore
    services.AddSingleton<Giraffe.Serialization.Json.IJsonSerializer>(ThothSerializer()) |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder.AddConsole()
           .AddDebug() |> ignore

[<EntryPoint>]
let main _ =
    WebHostBuilder()
        .UseKestrel()
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0
```


### Move blackjack to API
```fsharp
let blackjackHandler: HttpHandler =
    choose [
        GET >=> route "/begin" >=> warbler (fun _ -> (startGame () |> json))
        POST >=> choose [
            route "/hit" >=> bindJson<InProgressGame>(hit >> json)
            route "/stick" >=> bindJson<InProgressGame>(stick >> json)
        ]
    ]
```

```fsharp
let webApp =
    choose [
        subRoute "/api" (blackjackHandler)
        GET >=>
            choose [
                route "/" >=> greetingsHandler "world"
                routef "/hello/%s" greetingsHandler
                route "/ping"   >=> text "pong"
            ]
        setStatusCode 404 >=> text "Not Found" ]
```


# Client

### React hello world

```fsharp
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
open Browser

let helloWorld = div [ Class "helo-world" ] [
    str "Hello world from Fable!"
]

ReactDom.render(helloWorld, document.getElementById("elmish-app"))
```

### CSS type provider
```fsharp
type css = CssClasses<"styles.css", Naming.PascalCase>

let helloWorld = div [ Class css.HelloWorld ] [
    str "Hello world from Fable!"
]
```

### Shared code
```fsharp
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
```

### Add API definition for dealCard
Shared.fs:
```fsharp
let apiRouteBuilder typeName methodName =
    sprintf "/api/%s" methodName

type BlackjackApi = {
    dealCard: unit -> Async<Card>
}
```

Server.fs:
```fsharp
let remotingWebApp =
    Remoting.createApi()
    |> Remoting.fromValue blackJackApi
    |> Remoting.withRouteBuilder apiRouteBuilder
    |> Remoting.buildHttpHandler

let configureApp (app : IApplicationBuilder) =
    app.UseDeveloperExceptionPage()
        .UseStaticFiles()
        .UseGiraffe remotingWebApp
```

Client.fs:
```fsharp
let blackjackApi =
  Remoting.createApi()
  |> Remoting.withRouteBuilder apiRouteBuilder
  |> Remoting.buildProxy<BlackjackApi>

let dealCard = blackjackApi.dealCard
```


### Implement model
Copy code from Server:
```fsharp

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
```

Update to handle async:
```fsharp

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
```

### Model
```fsharp
type ApiRequest =
    | NotStarted
    | Loading
    | Error of exn

type Model = {
    Game: Game option
    Request: ApiRequest
}
```


### Message
```fsharp
type Message =
    | StartGame
    | StartGameSucceeded of InProgressGame
    | Hit
    | HitSucceeded of Game
    | Stick
    | StickSucceeded of CompletedGame
    | RequestFailed of exn
```

### Init
```fsharp
let init () =
    { Game = None
      Request = NotStarted }, Cmd.none
```

### Update
```fsharp
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
  ```

### Render our app
```fsharp
let view (model: Model) (dispatch: Message -> unit) =
  div [] [ "Hello world from Elmish" ]
  
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
```

### Render cards
```fsharp
importAll "@fortawesome/fontawesome-free/css/all.css"

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
```

### Render deck
```fsharp
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
```

### Storybook
```fsharp
module Stories

open Storybook
open Client
open Shared
open Fable.React
open Fable.Core.JsInterop

importAll "./styles.css"
importAll "@fortawesome/fontawesome-free/css/all.css"

let allCards: Card seq = seq {
    for suit in [ Spades; Diamonds; Hearts; Clubs ] do
        for rank in [ Value 2; Value 3; Value 4; Value 5; Value 6; Value 7; Value 8; Value 9; Value 10; Jack; Queen; King; Ace; ] do
            yield (rank, suit)
}

let exampleDeck = {
    Deck = {
        Cards = [(Jack, Spades); (Value 2, Hearts); (Value 10, Diamonds); (Ace, Clubs)]
        Score = 12;
    }
}

storiesOf("Blackjack", webpackModule)
    .add("Single card", (fun _ -> card { Card = (Jack, Spades) }))
    .add("All cards", (fun _ -> div [] [
        allCards |> Seq.map (fun c -> card { Card = c }) |> Seq.toList |> ofList
    ]))
    .add("Deck", (fun _ -> deck exampleDeck))
    |> ignore
```


### Put it all together
```fsharp

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
```