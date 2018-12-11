namespace Bandog.Core.Domain

module MusicSearch =
    open DomainTypes

    type SearchCriteria =
        { Skills : Map<Skill, SkillLevel list>
          Genre : Genre Set
          Location : Location }

    let rockInstruments =
        [
            Guitar Electric
            Drums
            Bass Electric
            Keyboard
            Vocals
        ] |> Set.ofList

    let metalInstruments = rockInstruments

    let jazzInstruments =
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

    let folkInstruments =
        [
            Violin
            Flute
            Guitar Acoustic
            Drums
            Vocals
            Cello
        ] |> Set.ofList

    let bluesInstruments =
        [
            Guitar Electric
            Vocals
            Bass Electric
            Drums
        ] |> Set.ofList

    let hiphopInstruments =
        [
            Vocals
            Drums
            Bass Electric
        ] |> Set.ofList

    let classicInstruments =
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

    let soulInstruments =
        [
            Keyboard
            Bass Electric
            Drums
            Guitar Electric
            Sax
        ] |> Set.ofList

    let instrumentsByGenre =
        function
        | Metal _ -> metalInstruments |> Some
        | Rock _ -> rockInstruments |> Some
        | Jazz _ -> jazzInstruments |> Some
        | Folk -> folkInstruments |> Some
        | HipHop _ -> hiphopInstruments |> Some
        | Blues -> bluesInstruments |> Some
        | Classic -> classicInstruments |> Some
        | Soul -> soulInstruments |> Some
        | _ -> None

    let getClosestLevels =
        function
        | Novice -> [ Novice; Apprentice ]
        | Apprentice -> [ Novice; Apprentice; Adept ]
        | Adept -> [ Apprentice; Adept; Maester ]
        | Maester -> [ Adept; Maester; GrandMaester ]
        | GrandMaester -> [ Maester; GrandMaester ]

    let inline median values =
        match values with
        | [||] | null -> None
        | [|x|] -> Some x
        | arr ->
            let arr = Array.sort arr
            arr.[arr.Length / 2] |> Some

    let averageLevel levels =
        levels
        |> Array.ofSeq
        |> median
        |> Option.defaultValue Novice

    let getSearchCriteriaByProfile (profile : MusicProfile) =
        let averageSkillLevelOfUser =
            profile.Skills
            |> Map.toList |> List.map snd |> averageLevel
        let instrumentsInterestingForUser =
            profile.Genres
            |> Seq.map instrumentsByGenre
            |> Seq.filter Option.isSome
            |> Seq.map Option.get
            |> Seq.concat
        let skillsWithLevels =
            instrumentsInterestingForUser
            |> Seq.map (fun s ->
                Instrument s,
                (Instrument s
                 |> profile.Skills.TryFind |> Option.defaultValue averageSkillLevelOfUser) |> getClosestLevels)
            |> Map.ofSeq
        { Skills = skillsWithLevels;
          Location = profile.UserInfo.Location;
          Genre = profile.Genres }

