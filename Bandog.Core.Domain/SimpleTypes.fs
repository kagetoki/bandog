namespace Bandog.Core.Domain

[<AutoOpen>]
module SimpleTypes =
    open System
    open System.Text.RegularExpressions

    let private lowestBirthDate = DateTimeOffset(1940, 1, 1, 0, 0, 0, TimeSpan.Zero)

    let private emailRegex = Regex("""\A[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\z""", RegexOptions.Compiled)

    let private nonLettersRegex = Regex("[^A-Za-z]", RegexOptions.Compiled)
    let private specialCharsRegex = Regex("\\W", RegexOptions.Compiled)
    let private nonDigitsRegex = Regex("\\D", RegexOptions.Compiled)

    let both f1 f2 input =
        if f1 input then f2 input
        else false

    let neither f1 f2 input =
        if f1 input then false
        else not<<f2 <| input

    let notEmptyString = String.IsNullOrEmpty >> not
    let notWhiteSpaceString = String.IsNullOrWhiteSpace >> not
    let isEmptyString = String.IsNullOrEmpty
    let isWhiteSpaceString = String.IsNullOrWhiteSpace

    type ValidationError =
        | LettersOnlyAllowed
        | LettersAndDigitsOnlyAllowed
        | StringIsEmpty
        | ListIsEmpty
        | IllegalCharacters of string
        | DateIsOutOfRange of range: (DateTimeOffset * DateTimeOffset)
        | NumberIsOutOfRange of range: (float * float)
        | InvalidEmail
        //| InvalidPhone

    let private tryCreate validate ctor input =
        if validate input then ctor input |> Some
        else None

    let private create validate error ctor input =
        if validate input then ctor input |> Ok
        else error |> Error

    type NonEmptyString = private NonEmptyString of string
        with
        member this.Value = match this with NonEmptyString str -> str
        static member tryCreate str = tryCreate notWhiteSpaceString NonEmptyString str
        static member create str = create notWhiteSpaceString StringIsEmpty NonEmptyString str

    type LetterString = private LetterString of string
        with
        member this.Value = match this with LetterString str -> str
        static member create str = create (neither String.IsNullOrWhiteSpace nonLettersRegex.IsMatch)

    type LetterAndDigitString = private LetterAndDigitString of string
        with
        member this.Value = match this with LetterAndDigitString str -> str
        static member create str =
            create (neither isWhiteSpaceString specialCharsRegex.IsMatch) (IllegalCharacters "!@#$%^&*(){}[].,/|\\\"'`") LetterAndDigitString str
        static member tryCreate str =
            tryCreate (neither isWhiteSpaceString specialCharsRegex.IsMatch) LetterAndDigitString str

    type 'T NonEmptyList =
        { Head : 'T 
          Tail : 'T list }
          with
          static member ofList list =
            match list with
            | [] -> None
            | h::t -> { Head = h; Tail = t} |> Some
          member this.ToList() =
            this.Head :: this.Tail

    [<Struct>]
    type BirthDate = private BirthDate of DateTimeOffset
        with
        member this.Value = match this with BirthDate d -> d
        static member tryCreate currentDate value =
            if value > lowestBirthDate && value < currentDate then BirthDate value |> Some
            else None

    type Email = private Email of string
        with
        member this.Value = match this with Email str -> str
        static member create str = create (both notEmptyString emailRegex.IsMatch) InvalidEmail Email str
        static member tryCreate str =
            tryCreate (both notEmptyString emailRegex.IsMatch) Email str

    type PhoneNumber = private PhoneNumber of string
        with
        member this.Value = match this with PhoneNumber str -> str
        static member create str = 
            create notWhiteSpaceString StringIsEmpty PhoneNumber str 
        static member tryCreate str =
            tryCreate notWhiteSpaceString PhoneNumber str

