module Parameter

open System.Collections.Generic

type VD<'t> = { Default : 't; Value : 't }

type ValueAndDefault =
    | S of VD<string>
    | I of VD<int32>
    | B of VD<bool>

type ValuesAndDefaults =
    | Ss of VD<string array>
    | Is of VD<int32 array>
    | Bs of VD<bool array>

let replaceValueInClone (src: VD<'t array>) (idx: int) (elt: 't): VD<'t array> =
    let newArray = Array.copy(src.Value)
    Array.set newArray idx elt
    { src with Value = newArray }

type INamedParameter =
    abstract member Name: string
    abstract member UiName: string

type NamedParameter(name: string, uiName: string) =
    interface INamedParameter with
        member _.Name = name
        member _.UiName = uiName

type IValidatableParameter =
    inherit INamedParameter
    abstract member ValueAndDefault: ValueAndDefault
    abstract member ValidationRules: ValidationRule list
    abstract member Language: string option

and IndependentParameter(name: string,
                         uiName: string,
                         valueAndDefault: ValueAndDefault,
                         description: string,
                         validationRules: ValidationRule list) =
    interface IValidatableParameter with
        member _.Name = name
        member _.UiName = uiName
        member _.ValueAndDefault = valueAndDefault
        member _.ValidationRules = validationRules
        member _.Language = None
    member _.Description = description
    new(p: INamedParameter,
        valueAndDefault: ValueAndDefault,
        description: string,
        validationRules: ValidationRule list) =
        IndependentParameter(p.Name, p.UiName, valueAndDefault, description, validationRules)
    new(p: IValidatableParameter,
        description: string,
        validationRules: ValidationRule list) =
        IndependentParameter(p.Name, p.UiName, p.ValueAndDefault, description, validationRules)
    new(p: IndependentParameter, vad: ValueAndDefault) =
        let validatable = p :> IValidatableParameter
        match (validatable.ValueAndDefault, vad) with
        | (S _, S { Value = newValue }) -> IndependentParameter(p, newValue)
        | (I _, I { Value = newValue }) -> IndependentParameter(p, newValue)
        | (B _, B { Value = newValue }) -> IndependentParameter(p, newValue)
        | _ ->
            System.ArgumentException("Type error: trying to replace with incompatible value")
            |> raise
            // Make the compiler happy:
            IndependentParameter(
                "What is the compiler thinking?",
                "It should see that an error is raised so that this code",
                S { Default = "will"; Value = "never" },
                "be executed.",
                [])
    new(p: IndependentParameter, newValue: string) =
        let validatable = p :> IValidatableParameter
        let newValueAndDefault =
            match validatable.ValueAndDefault with
            | S oldVad ->
                S { oldVad with Value = newValue }
            | _ ->
                System.ArgumentException("Type error: trying to put string into incompatible parameter")
                |> raise
        IndependentParameter(
            validatable.Name,
            validatable.UiName,
            newValueAndDefault,
            p.Description,
            validatable.ValidationRules)
    new(p: IndependentParameter, newValue: int32) =
        let validatable = p :> IValidatableParameter
        let newValueAndDefault =
            match validatable.ValueAndDefault with
            | I oldVad ->
                I { oldVad with Value = newValue }
            | _ ->
                System.ArgumentException("Type error: trying to put int into incompatible parameter")
                |> raise
        IndependentParameter(
            validatable.Name,
            validatable.UiName,
            newValueAndDefault,
            p.Description,
            validatable.ValidationRules)
    new(p: IndependentParameter, newValue: bool) =
        let validatable = p :> IValidatableParameter
        let newValueAndDefault =
            match validatable.ValueAndDefault with
            | B oldVad ->
                B { oldVad with Value = newValue }
            | _ ->
                System.ArgumentException("Type error: trying to put boolean into incompatible parameter")
                |> raise
        IndependentParameter(
            validatable.Name,
            validatable.UiName,
            newValueAndDefault,
            p.Description,
            validatable.ValidationRules)

and LanguageParameter(name: string,
                      uiName: string,
                      valuesAndDefaults: ValuesAndDefaults,
                      description: string,
                      validationRules: ValidationRule list) =
    interface INamedParameter with
        member _.Name = name
        member _.UiName = uiName
    member _.ValidationRules = validationRules
    member _.Description = description
    member _.ValuesAndDefaults = valuesAndDefaults
    new(p: INamedParameter,
        valuesAndDefaults: ValuesAndDefaults,
        description: string,
        validationRules: ValidationRule list) =
        LanguageParameter(p.Name, p.UiName, valuesAndDefaults, description, validationRules)
    new(p: LanguageParameter, idx: int, vad: ValueAndDefault) =
        let named = p :> INamedParameter
        let newValuesAndDefaults =
            match (p.ValuesAndDefaults, vad) with
            | (Ss oldVads, S newVad) ->
                replaceValueInClone oldVads idx newVad.Value |> Ss
            | (Is oldVads, I newVad) ->
                replaceValueInClone oldVads idx newVad.Value |> Is
            | (Bs oldVads, B newVad) ->
                replaceValueInClone oldVads idx newVad.Value |> Bs
            | _ ->
                System.ArgumentException("Type error: trying to replace with incompatible value")
                |> raise
        LanguageParameter(
            named.Name,
            named.UiName,
            newValuesAndDefaults,
            p.Description,
            p.ValidationRules)
    new(p: LanguageParameter, idx: int, newValue: string) =
        let named = p :> INamedParameter
        let newValuesAndDefaults =
            match p.ValuesAndDefaults with
            | Ss oldVads ->
                replaceValueInClone oldVads idx newValue |> Ss
            | _ ->
                System.ArgumentException("Type error: trying to put string into incompatible parameter")
                |> raise
        LanguageParameter(
            named.Name,
            named.UiName,
            newValuesAndDefaults,
            p.Description,
            p.ValidationRules)
    new(p: LanguageParameter, idx: int, newValue: int32) =
        let named = p :> INamedParameter
        let newValuesAndDefaults =
            match p.ValuesAndDefaults with
            | Is oldVads ->
                replaceValueInClone oldVads idx newValue |> Is
            | _ ->
                System.ArgumentException("Type error: trying to put int into incompatible parameter")
                |> raise
        LanguageParameter(
            named.Name,
            named.UiName,
            newValuesAndDefaults,
            p.Description,
            p.ValidationRules)
    new(p: LanguageParameter, idx: int, newValue: bool) =
        let named = p :> INamedParameter
        let newValuesAndDefaults =
            match p.ValuesAndDefaults with
            | Bs oldVads ->
                replaceValueInClone oldVads idx newValue |> Bs
            | _ ->
                System.ArgumentException("Type error: trying to put boolean into incompatible parameter")
                |> raise
        LanguageParameter(
            named.Name,
            named.UiName,
            newValuesAndDefaults,
            p.Description,
            p.ValidationRules)

and MinimalParameter(name: string, uiName: string, valueAndDefault: ValueAndDefault, validationRules: ValidationRule list, forLanguage: string option) =
    interface IValidatableParameter with
        member _.Name = name
        member _.UiName = uiName
        member _.ValueAndDefault = valueAndDefault
        member _.ValidationRules = validationRules
        member _.Language = forLanguage
    new(p: IValidatableParameter) = MinimalParameter(p.Name, p.UiName, p.ValueAndDefault, p.ValidationRules, None)
    new(p: LanguageParameter, idx: int, language: string) =
        let valAndDeflt =
            match p.ValuesAndDefaults with
            | Ss { Default = defaults ; Value = values } -> S { Default = defaults.[idx]; Value = values.[idx] }
            | Is { Default = defaults ; Value = values } -> I { Default = defaults.[idx]; Value = values.[idx] }
            | Bs { Default = defaults ; Value = values } -> B { Default = defaults.[idx]; Value = values.[idx] }
        MinimalParameter((p :> INamedParameter).Name, (p :> INamedParameter).UiName, valAndDeflt, p.ValidationRules, Some language)

and ValidationRule = IValidatableParameter list -> string

let tryGetString (p: IValidatableParameter): Result<string, string> =
    match p.ValueAndDefault with
    | S { Value = v } -> Ok v
    | _ -> sprintf "Type error: '%s' is not a string." p.UiName |> Error

let tryGetInt32 (p: IValidatableParameter): Result<int32, string> =
    match p.ValueAndDefault with
    | I { Value = v } -> Ok v
    | _ -> sprintf "Type error: '%s' is not a number." p.UiName |> Error

let tryGetBool (p : IValidatableParameter) : Result<bool, string> =
    match p.ValueAndDefault with
    | B { Value = v } -> Ok v
    | _ -> sprintf "Type error: '%s' is not a boolean." p.UiName |> Error

let rec extractValue tryGet name (ps: IValidatableParameter list) =
    match List.tryFind (fun (p: IValidatableParameter) -> p.Name = name) ps with
    | None -> sprintf "'%s' not found." name |> Error
    | Some p -> tryGet p

let extractString = extractValue tryGetString

let extractInt32 = extractValue tryGetInt32

let extractBool = extractValue tryGetBool

let applyOnOK defaultValue f = function
    | Ok v -> f v
    | Error _ -> defaultValue

let replaceByCloneIfNameMatch<'n when 'n :> INamedParameter> (name: string) (clone: 'n -> 'n) =
    let f p =
        let named = p :> INamedParameter
        if named.Name = name
        then clone(p)
        else p
    List.map f

type ParameterMetadata =
    {
        name: string
        uiName: string
        description: string
        type': string // "string" | "int" | "bool"
        prefix: string option // only makes sense for type' = "string"
        min: int option // min length for "string", minimum value for "int"
        max: int option // max length for "string", maximum value for "int"
        value: string // is parsed...
    }

let validateHasStringPrefix (prefix: string) (p: INamedParameter): ValidationRule =
    let rule (ps: IValidatableParameter list) =
        extractString p.Name ps
        |> applyOnOK (sprintf "Type error: '<<%s>>' is not a string." p.Name)
                     (fun s -> if s.StartsWith(prefix)
                               then ""
                               else sprintf "'<<%s>>' should start with '%s'." p.Name prefix)
    rule

let validateHasMinMaxLength (min: int) (max: int) (p: INamedParameter): ValidationRule =
    let rule (ps: IValidatableParameter list) =
        extractString p.Name ps
        |> applyOnOK (sprintf "Type error: '<<%s>>' is not a string." p.Name)
                     (fun s -> let len = s.Length
                               if ((min <= len) && (len <= max))
                               then ""
                               else sprintf "'<<%s>>' should be between %d and %d characters." p.Name min max)
    rule

let validateHasMinMaxValue (min: int) (max: int) (p: INamedParameter): ValidationRule =
    let rule (ps: IValidatableParameter list) =
        extractInt32 p.Name ps
        |> applyOnOK (sprintf "Type error: '<<%s>>' is not an int." p.Name)
                     (fun v -> if ((min <= v) && (v <= max))
                               then ""
                               else sprintf "'<<%s>>' should be between %d and %d." p.Name min max)
    rule

let validateParameterMetadata (existing: HashSet<string>) pmDirty =
    match pmDirty with
    | { name = n } when System.String.IsNullOrWhiteSpace(n) ->
        Error "name is blank"
    | { description = d } when System.String.IsNullOrWhiteSpace(d) ->
        Error "description is blank"
    | { type' = t } when not (t = "string" || t = "int" || t = "bool") ->
        if System.String.IsNullOrWhiteSpace(t)
        then "type' is blank"
        else sprintf "type'=%s is unknown" t
        |> Error
    | { type' = t; prefix = Some _ } when t <> "string" ->
        sprintf "a prefix does not make sense for type'=%s, only for string" t
        |> Error
    | { type' = "bool"; min = Some _ }
    | { type' = "bool"; max = Some _ } ->
        Error "min and max are not supported for type'=bool"
    // Order of clauses matters because of this side effect: adds parameter name to set
    | { name = n } when existing.Add(n) = false ->
        Error "duplicate name %s"
    | _ ->
        Ok pmDirty

let tieValidationRules
        (name: string)
        (uiName: string)
        (parameterBuilder: INamedParameter -> ValidationRule list -> 'param)
        (rulesBuilder: INamedParameter -> ValidationRule list)
        : 'param =
    let p = NamedParameter(name, uiName)
    rulesBuilder p
    |> parameterBuilder p

let tryCreateLanguageIndependent existing (pmDirty: ParameterMetadata): Result<IndependentParameter, string> =
    let pmOrError = validateParameterMetadata existing pmDirty
    let buildStringValidationRules (pm: ParameterMetadata) (p: INamedParameter): ValidationRule list =
        List.concat [
            match pm.prefix with
            | Some s when s <> "" -> [validateHasStringPrefix pm.prefix.Value p]
            | _ -> []
            match pm.max, pm.min with
            | (Some upperLength, Some lowerLength) ->
                [validateHasMinMaxLength lowerLength upperLength p]
            | (Some upperLength, None) ->
                [validateHasMinMaxLength 0 upperLength p]
            | (None, Some lowerLength) ->
                [validateHasMinMaxLength lowerLength System.Int32.MaxValue p]
            | (None, None) -> []]
    let buildIntValidationRules (pm: ParameterMetadata) (p: INamedParameter): ValidationRule list =
        match pm.max, pm.min with
        | (Some upperValue, Some lowerValue) ->
            [validateHasMinMaxValue lowerValue upperValue p]
        | (Some upperValue, None) ->
            [validateHasMinMaxValue System.Int32.MinValue upperValue p]
        | (None, Some lowerValue) ->
            [validateHasMinMaxValue lowerValue System.Int32.MaxValue p]
        | (None, None) -> []
    Result.bind (fun pm ->
                 let buildParameter vad rulesBuilder =
                     let parameterBuilder p rules = IndependentParameter(p, vad, pm.description, rules)
                     rulesBuilder pm
                     |> tieValidationRules pm.name pm.uiName parameterBuilder
                 match pm with
                 | { type' = "string" } ->
                     buildParameter
                         (S { Value = pm.value; Default = pm.value })
                         buildStringValidationRules
                     |> Ok
                 | { type' = "int" } ->
                     match System.Int32.TryParse pm.value with
                     | (true, value) ->
                         buildParameter
                             (I { Value = value; Default = value })
                             buildIntValidationRules
                          |> Ok
                     | (false, _) ->
                         sprintf "can't parse %s to %s" pm.value pm.type'
                         |> Error
                 | { type' = "bool"; value = v } ->
                     if v = "true" || v = "false"
                     then let value = v = "true"
                          IndependentParameter(
                             pm.name,
                             pm.uiName,
                             (B { Value = value; Default = value }),
                             pm.description,
                             [])
                          |> Ok
                     else sprintf "%s must be true or false for a %s" v pm.type'
                          |> Error
                 | _ -> Error "unknown type (NOTREACHED)")
                pmOrError
    |> Result.mapError (fun s ->
                        let name = match pmOrError with
                                   | Ok { name = n } -> n
                                   | Error _ when not(System.String.IsNullOrWhiteSpace pmDirty.name) ->
                                       pmDirty.name
                                   | Error _ when not(System.String.IsNullOrWhiteSpace pmDirty.uiName) ->
                                       pmDirty.uiName
                                   | Error _ when not(System.String.IsNullOrWhiteSpace pmDirty.description) ->
                                       pmDirty.description
                                   | Error _ -> ""
                        let blank = if System.String.IsNullOrWhiteSpace name
                                    then ""
                                    else " "
                        sprintf "Error while validating%s%s metadata: %s." blank name s)

let tryCreateLanguageDependent existing (count: int) (pmDirty: ParameterMetadata): Result<LanguageParameter, string> =
    tryCreateLanguageIndependent existing pmDirty
    |> Result.map
        (fun p ->
         let validatable = p :> IValidatableParameter
         let arrayify wrap { Value = v ; Default = d } =
             wrap { Value = Array.create count v; Default = Array.create count d }
         let valuesAndDefaults =
             match validatable.ValueAndDefault with
             | S vad -> arrayify Ss vad
             | I vad -> arrayify Is vad
             | B vad -> arrayify Bs vad
         LanguageParameter(p,
                           valuesAndDefaults,
                           p.Description,
                           validatable.ValidationRules))

let traverse (f: 'a -> Result<'ok, 'err>) (xs: 'a list): Result<'ok list, 'err> =
    let rec go accumulator = function
        | [] -> List.rev accumulator |> Ok
        | x::tail -> match f x with
                     | Ok a -> go (a::accumulator) tail
                     | Error b -> Error b
    go [] xs

let sequence (ss: Result<'a, 'b> list): Result<'a list, 'b> =
    let rec go accumulator = function
        | [] -> List.rev accumulator |> Ok
        | Error b::_ -> Error b
        | Ok a::tail -> go (a::accumulator) tail
    go [] ss
