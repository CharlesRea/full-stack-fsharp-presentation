module Stories

open Storybook
open Client
open Shared
open Fable.React
open Fable.Core.JsInterop

importAll "./styles.css"
importAll "@fortawesome/fontawesome-free/css/all.css"

let allCards = seq {
    for suit in [ Spades; Diamonds; Hearts; Clubs ] do
        for rank in [ Value 2; Value 3; Value 4; Value 5; Value 6; Value 7; Value 8; Value 9; Value 10; Jack; Queen; King; Ace; ] do
            yield { Rank = rank; Suit = suit; }
}

storiesOf("Blackjack", webpackModule)
    .add("Single card", (fun _ -> card { Card = { Rank = Jack ; Suit = Spades } }))
    .add("All cards", (fun _ -> div [] [
        allCards |> Seq.map (fun c -> card { Card = c }) |> Seq.toList |> ofList
    ]))
    .add("Deck", (fun _ -> deck { Deck = [{ Rank = Jack ; Suit = Spades }; { Rank = Value 2; Suit = Hearts; }; { Rank = Value 10; Suit = Clubs; }] }))
    |> ignore
