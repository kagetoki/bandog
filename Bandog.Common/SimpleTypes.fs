namespace Bandog.Common

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

    let inline notNull a = (not<<isNull) a
    let notEmptyString = String.IsNullOrEmpty >> not
    let notWhiteSpaceString = String.IsNullOrWhiteSpace >> not
    let isEmptyString = String.IsNullOrEmpty
    let isWhiteSpaceString = String.IsNullOrWhiteSpace
    let inline (=~) str1 str2 = String.Equals(str1, str2, StringComparison.InvariantCultureIgnoreCase)

    type nil<'A when 'A : struct and 'A: (new: unit -> 'A) and 'A :> ValueType> = Nullable<'A>

    type ValidationError =
        | LettersOnlyAllowed
        | LettersAndDigitsOnlyAllowed
        | StringIsEmpty
        | ListIsEmpty
        | IllegalCharacters of string
        | DateIsOutOfRange of range: (DateTimeOffset * DateTimeOffset)
        | NumberIsOutOfRange of range: (float * float)
        | InvalidEmail
        | InvalidInput of message:string

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
        static member create str = create (neither String.IsNullOrWhiteSpace nonLettersRegex.IsMatch) LettersOnlyAllowed LetterString str

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
          static member ofList (list: 'T list) =
            match list with
            | [] -> None
            | h::t -> { Head = h; Tail = t} |> Some
          member this.ToList() =
            this.Head :: this.Tail
          member this.Add item =
            { Head = item; Tail = this.ToList() }

    type 'T NonEmptySet when 'T : comparison =
        private NonEmptySet of 'T Set
        with
        member this.Value = match this with | NonEmptySet set -> set
        member this.ToList() = this.Value |> List.ofSeq
        member this.Add item =
          match this with | NonEmptySet set -> NonEmptySet <| set.Add item
        member this.Remove item =
          match this with
          | NonEmptySet set ->
            let newSet = set.Remove item
            if newSet.IsEmpty then set else newSet
            |> NonEmptySet
        static member ofSeq s =
            let set = Set.ofSeq s
            if set.IsEmpty then None
            else NonEmptySet set |> Some

    [<Struct>]
    type BirthDate = private BirthDate of DateTimeOffset
        with
        member this.Value = match this with BirthDate d -> d
        static member create currentDate value =
            if value > lowestBirthDate && value < currentDate then BirthDate value |> Ok
            else DateIsOutOfRange (lowestBirthDate, currentDate) |> Error
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

