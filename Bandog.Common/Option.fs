namespace Bandog.Common

[<RequireQualifiedAccess>]
module Option =

    let ofString =
        function
        | (null | "") -> None
        | str -> Some str
