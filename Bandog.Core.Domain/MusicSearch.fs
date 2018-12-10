namespace Bandog.Core.Domain

module MusicSearch =
    open DomainTypes

    type SearchCriteria =
        { Skills : Map<Skill, SkillLevel list>
          Genre : Genre Set
          Location : Location }

    let private levelToInt =
        function
        | Novice -> 1
        | Apprentice -> 2
        | Adept -> 3
        | Maester -> 4
        | GrandMaester -> 5

    let levelFromInt =
        function
        | 1 -> Novice |> Some
        | 2 -> Apprentice |> Some
        | 3 -> Adept |> Some
        | 4 -> Maester |> Some
        | 5 -> GrandMaester |> Some
        | _ -> None

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

    let averageLevel levels =
        levels
        |> Seq.map (levelToInt >> float)
        |> Seq.average
        |> (int >> levelFromInt >> Option.get)

    let getSearchCriteriaByProfile (profile : MusicProfile) =
        let averageSkillLevelOfUser =
            profile.Skills
            |> Map.toList |> List.map snd |> averageLevel
        let instrumentsInterestedForUser =
            profile.Genres
            |> Seq.map playersByGenre
            |> Seq.filter Option.isSome
            |> Seq.map Option.get
            |> Seq.concat
        let skillsWithLevels =
            instrumentsInterestedForUser
            |> Seq.map (fun s ->
                Instrument s, (Instrument s |> profile.Skills.TryFind |> Option.defaultValue averageSkillLevelOfUser) |> getClosestLevels)
            |> Map.ofSeq
        { Skills = skillsWithLevels;
          Location = profile.UserInfo.Location;
          Genre = profile.Genres }
