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
