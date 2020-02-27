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
