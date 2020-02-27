module Database

open FSharp.Data.Npgsql
open System

[<Literal>]
let connString = "Host=localhost;Database=giraffe;Username=postgres;Password=password;"

type Connection = NpgsqlConnection<connString>

let getUser id =
    use cmd = Connection.CreateCommand<"
    SELECT * from users where user_id = @id
", SingleRow=true>(connString)

    match cmd.Execute id with
           | Some result ->
               printf "user name: %s" result.name
           | None -> failwith "invalid id"

let createUser (name: string) =
    use cmd = Connection.CreateCommand<"
        INSERT INTO users (name, created_date) VALUES (@name, @createdAt) RETURNING user_id
    ", SingleRow=true>(connString)

    match cmd.Execute(name = name, createdAt = DateTime.UtcNow) with
           | Some id -> printf "Created user with ID %d" id
           | None -> failwith "Failed to insert records"