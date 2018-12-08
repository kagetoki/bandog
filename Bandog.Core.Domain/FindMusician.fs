namespace Bandog.Core.Domain

module FindMusician =
    open DomainTypes

    type SearchCriteria =
        { Skills : Map<Skill, SkillLevel>
          Genre : Genre Set
          Location : Location }

    let rockBandPlayers =
        [
            Guitar Electric
            Drums
            Bass Electric
            Keyboard
            Vocals
        ] |> Set.ofList

    let metalBandPlayers = rockBandPlayers

    let jazzPlayers =
        [
            Keyboard
            Bass Electric
            Bass Acoustic
            Drums
            Vocals
            Sax
            Clarinet
            Guitar Electric
            Guitar Acoustic
        ] |> Set.ofList

    let folkPlayers =
        [
            Violin
            Flute
            Guitar Acoustic
            Drums
            Vocals
            Cello
        ] |> Set.ofList

    let bluesPlayers =
        [
            Guitar Electric
            Vocals
            Bass Electric
            Drums
        ] |> Set.ofList

    let hiphopPlayers =
        [
            Vocals
            Drums
            Bass Electric
        ] |> Set.ofList

    let classicPlayers =
        [
            Keyboard
            Violin
            Cello
            Flute
            Clarinet
            Bass Acoustic
            Guitar Acoustic
            Trombone
            Drums
        ] |> Set.ofList

    let soulPlayers =
        [
            Keyboard
            Bass Electric
            Drums
            Guitar Electric
            Sax
        ] |> Set.ofList

    let playersByGenre =
        function
        | Metal _ -> metalBandPlayers |> Some
        | Rock _ -> rockBandPlayers |> Some
        | Jazz _ -> jazzPlayers |> Some
        | Folk -> folkPlayers |> Some
        | HipHop _ -> hiphopPlayers |> Some
        | Blues -> bluesPlayers |> Some
        | Classic -> classicPlayers |> Some
        | Soul -> soulPlayers |> Some
        | _ -> None

    let getClosestLevels =
        function
        | Novice -> [ Novice; Apprentice ]
        | Apprentice -> [ Novice; Apprentice; Adept ]
        | Adept -> [ Apprentice; Adept; Maester ]
        | Maester -> [ Adept; Maester; GrandMaester ]
        | GrandMaester -> [ Maester; GrandMaester ]

    //let getSearchCriteriaBy (profile : MusicProfile) =
        
