module App

open Browser.Dom

printf "Hello from Fable!"

let mutable count = 0

let counterButton = document.querySelector(".my-button") :?> Browser.Types.HTMLButtonElement

counterButton.onclick <- fun _ ->
    count <- count + 1
    counterButton.innerText <- sprintf "You clicked: %i time(s)" count

