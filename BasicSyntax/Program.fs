// Learn more about F# at http://fsharp.org

open System

let square x = x * x

let nine = square 3

let sumOfSquares = List.sum (List.map square [ 1 .. 100 ])

let piped =
    [ 1 .. 100 ]
    |> List.map square
    |> List.sum

let lambda =
    [ 1 .. 100 ]
    |> List.map (fun x -> x * x)
    |> List.sum

let tuple = (1, 2, 3)

let sendEmail email =
    ignore

let sendSms number =
    ignore

type ContactMethod =
    | Email of string
    | Phone of phoneNumber: int * extension: int option

type Person =
    { name: string
      contactDetails: ContactMethod }

let bob =
    { name = "Bob Robertson"
      contactDetails = Email "box@examle.com" }

let sendMessage (person: Person) =
    match person.contactDetails with
    | Email emailAddress -> sendEmail emailAddress
    | Phone(number, _) -> sendSms number

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
