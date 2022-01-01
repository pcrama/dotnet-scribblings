module bolerogator.Client.Main

open Elmish
open Bolero
open Bolero.Html
open Bolero.Remoting
open Bolero.Remoting.Client
open Bolero.Templating.Client

open Parameter

/// Routing endpoints definition.
type Page =
    | [<EndPoint "/">] Home
    | [<EndPoint "/general">] General
    | [<EndPoint "/common">] Common
    | [<EndPoint "/data">] Data

/// The Elmish application's model.
type Model =
    {
        page: Page
        configurationMetadatas: ConfigurationMetadata[] option
        configuration: Configuration option
        error: string option
        username: string
        password: string
        signedInAs: option<string>
        signInFailed: bool
    }

and Configuration =
    {
        configurationProjectName: string
        Languages: string list
        LanguageDependent: LanguageParameter list
        Parameters: IndependentParameter list
    }

and ConfigurationMetadata =
    {
        name: string
        languageIndependent: ParameterMetadata list
        languageDependent: ParameterMetadata list
    }

let initModel =
    {
        page = Home
        configurationMetadatas = None
        configuration = None
        error = None
        username = ""
        password = ""
        signedInAs = None
        signInFailed = false
    }

/// Remote service definition.
type ConfigurationMetadataService =
    {
        /// Get the list of all configurationMetadatas in the collection.
        getConfigurationMetadatas: unit -> Async<ConfigurationMetadata[]>

        /// Sign into the application.
        signIn : string * string -> Async<option<string>>

        /// Get the user's name, or None if they are not authenticated.
        getUsername : unit -> Async<string>

        /// Sign out from the application.
        signOut : unit -> Async<unit>
    }

    interface IRemoteService with
        member _.BasePath = "/configurationMetadatas"

/// The Elmish application's update messages.
type Message =
    | SetPage of Page
    | GetConfigurationMetadatas
    | GotConfigurationMetadatas of ConfigurationMetadata[]
    | CreateNewConfiguration of ConfigurationMetadata
    | SetLanguageOrder of string list
    | SetProjectName of string
    | SetBool of string * string Option * bool
    | SetString of string * string Option * string
    | SetInt32 of string * string Option * int32
    | SetUsername of string
    | SetPassword of string
    | GetSignedInAs
    | RecvSignedInAs of option<string>
    | SendSignIn
    | RecvSignIn of option<string>
    | SendSignOut
    | RecvSignOut
    | Error of exn
    | ClearError

let tryCreateFreshConfiguration conf name =
    traverse tryCreateLanguageIndependent conf.languageIndependent
    |> Result.bind
        (fun languageIndependents ->
         let languages = ["English"; "Dutch"; "German"; "French"; "Spanish"; "Italian"]
         conf.languageDependent
         |> (List.length languages |> tryCreateLanguageDependent |> traverse)
         |> Result.map
             (fun languageDependents ->
              {
                  configurationProjectName = name
                  Languages = languages
                  Parameters = languageIndependents
                  LanguageDependent = languageDependents
              }))

let findLanguage lang =
    List.findIndex (fun x -> x = lang)

let reshuffleLanguages
        (oldLangs: string list)
        (data: LanguageParameter list)
        (newLangs : string list)
        : LanguageParameter list =
    let indices = List.map (fun newLang -> findLanguage newLang oldLangs) newLangs
    let shuffle (xs: 'a array): 'a array =
        [| for idx in indices -> xs.[idx] |]
    let shuffleVD { Default = ds; Value = vs } = { Default = shuffle ds; Value = shuffle vs }
    let shuffleValuesAndDefaults = function
        | Ss ss -> shuffleVD ss |> Ss
        | Is is -> shuffleVD is |> Is
        | Bs bs -> shuffleVD bs |> Bs
    let shuffleParameter (p: LanguageParameter) =
        new LanguageParameter(
            p,
            shuffleValuesAndDefaults p.ValuesAndDefaults,
            p.Description,
            p.ValidationRules)
    List.map shuffleParameter data

let updateConfigurationInModel model (uc: Configuration -> Configuration): Model * Cmd<Message> =
    match model with
    | { configuration = Some configuration } ->
        { model with configuration = uc configuration |> Some }
        , Cmd.none
    | { configuration = None } ->
        model
        , System.Exception("No configuration to update.") |> Error |> Cmd.ofMsg

let replaceIndependentParameters (model: Model) (name: string) (clone: IndependentParameter -> IndependentParameter) =
    updateConfigurationInModel model <| fun conf ->
        {conf with Parameters = replaceByCloneIfNameMatch name clone conf.Parameters }

let replaceLanguageParameters (model: Model) (language: string) (name: string) (clone: int -> LanguageParameter -> LanguageParameter) =
    updateConfigurationInModel model <| fun conf ->
        let idx = findLanguage language conf.Languages
        { conf with LanguageDependent = replaceByCloneIfNameMatch name (clone idx) conf.LanguageDependent }

let update remote _jsRuntime message model =
    let onSignIn = function
        | Some _ -> Cmd.ofMsg GetConfigurationMetadatas
        | None -> Cmd.none
    match message with
    | SetPage page ->
        { model with page = page }, Cmd.none

    | GetConfigurationMetadatas ->
        let cmd = Cmd.OfAsync.either remote.getConfigurationMetadatas () GotConfigurationMetadatas Error
        { model with configurationMetadatas = None }, cmd
    | GotConfigurationMetadatas data ->
        { model with configurationMetadatas = Some data }, Cmd.none

    | CreateNewConfiguration conf ->
        let projectNamePrefix = match model.signedInAs with
                                | None -> ""
                                | Some userName -> sprintf "%s " userName
        match sprintf "%s%s" projectNamePrefix conf.name |> tryCreateFreshConfiguration conf with
        | Result.Ok configuration ->
            { model with configuration = Some configuration; error = None }
        | Result.Error message ->
            { model with error = Some message; configuration = None }
        , Cmd.none
    | SetLanguageOrder newLanguages ->
        updateConfigurationInModel model <| fun configuration ->
            let newDependents = reshuffleLanguages configuration.Languages
                                                   configuration.LanguageDependent
                                                   newLanguages
            { configuration with
                  Languages = newLanguages
                  LanguageDependent = newDependents }
    | SetProjectName newProjectName ->
        updateConfigurationInModel model <| fun configuration ->
            { configuration with configurationProjectName = newProjectName }
    | SetString (name, None, newValue) ->
        let clone p = IndependentParameter(p, newValue)
        replaceIndependentParameters model name clone
    | SetString (name, Some language, newValue) ->
        let clone idx p = LanguageParameter(p, idx, newValue)
        replaceLanguageParameters model language name clone
    | SetInt32 (name, None, newValue) ->
        let clone p = IndependentParameter(p, newValue)
        replaceIndependentParameters model name clone
    | SetInt32 (name, Some language, newValue) ->
        let clone idx p = LanguageParameter(p, idx, newValue)
        replaceLanguageParameters model language name clone
    | SetBool (name, None, newValue) ->
        let clone p = IndependentParameter(p, newValue)
        replaceIndependentParameters model name clone
    | SetBool (name, Some language, newValue) ->
        let clone idx p = LanguageParameter(p, idx, newValue)
        replaceLanguageParameters model language name clone

    | SetUsername s ->
        { model with username = s }, Cmd.none
    | SetPassword s ->
        { model with password = s }, Cmd.none
    | GetSignedInAs ->
        model, Cmd.OfAuthorized.either remote.getUsername () RecvSignedInAs Error
    | RecvSignedInAs username ->
        { model with signedInAs = username }, onSignIn username
    | SendSignIn ->
        model, Cmd.OfAsync.either remote.signIn (model.username, model.password) RecvSignIn Error
    | RecvSignIn username ->
        { model with signedInAs = username; signInFailed = Option.isNone username }, onSignIn username
    | SendSignOut ->
        model, Cmd.OfAsync.either remote.signOut () (fun () -> RecvSignOut) Error
    | RecvSignOut ->
        { model with signedInAs = None; signInFailed = false; configuration = None }, Cmd.none

    | Error RemoteUnauthorizedException ->
        { model with error = Some "You have been logged out."; signedInAs = None }, Cmd.none
    | Error exn ->
        { model with error = Some exn.Message }, Cmd.none
    | ClearError ->
        { model with error = None }, Cmd.none

/// Connects the routing system to the Elmish application.
let router = Router.infer SetPage (fun model -> model.page)

type Main = Template<"wwwroot/main.html">

let configurationProjectName = function
    | { configuration = Some { configurationProjectName = configurationProjectName } } ->
        configurationProjectName
    | { configuration = None } -> "No Project yet."

let homePage model _dispatch =
    Main.Home()
        .Title(configurationProjectName model)
        .Elt()

let getInputId p l =
    match l with
    | None -> (p :> INamedParameter).Name |> sprintf "input-common-%s"
    | Some n -> (p :> INamedParameter).Name |> sprintf "input-%s-%s" n

let limitingToInt32OnChange (setInt32: int -> Message) (onChange: Message -> unit) (newValue: int): unit =
    printfn "New value: %d" newValue
    if (System.Int32.MinValue <= newValue) && (newValue <= System.Int32.MaxValue)
    then newValue |> setInt32 |> onChange
    else System.Exception(sprintf "%d overflows." newValue) |> Error |> onChange

let renderParameterInput
        (p: MinimalParameter)
        (id: string)
        (setString: string -> Message)
        (setInt32: int -> Message)
        (setBool: bool -> Message)
        (onChange: Message -> unit) =
    match (p :> IValidatableParameter).ValueAndDefault with
    | I { Value = i } ->
        Main.NumberInput()
            .Id(id)
            .Value(i, limitingToInt32OnChange setInt32 onChange)
            .Elt()
    | S { Value = s } ->
        Main.TextInput()
            .Id(id)
            .Value(s,
                   fun s ->
                       printfn "Changing string %s to %s" (p :> INamedParameter).Name s
                       setString s |> onChange)
            .Elt()
    | B { Value = b } ->
        Main.BoolInput()
            .Id(id)
            .Value(b, setBool >> onChange)
            .Elt()

let commonPage model dispatch =
    match model with
    | { configuration = None } -> text "Internal error."
    | { configuration = Some configuration } ->
        Main.Common()
            .IndependentParameters(
                forEach configuration.Parameters <| fun p ->
                    let inputId = getInputId p None
                    let named = p :> INamedParameter
                    li [] [
                        label [attr.``for`` inputId] [text p.Description]
                        renderParameterInput (new MinimalParameter(p))
                                             inputId
                                             (fun v -> SetString (named.Name, None, v))
                                             (fun i -> SetInt32 (named.Name, None, i))
                                             (fun b -> SetBool (named.Name, None, b))
                                             dispatch])
            .Elt()

let generalPage model dispatch =
    let removeIdx idx xs =
        List.indexed xs
        |> List.filter (fun (otherIdx, _) -> otherIdx <> idx)
        |> List.map snd
    let languages =
        match model with
        | { configuration = Some { Languages = languageNames } } ->
            let mayNotRemove = match languageNames with
                               | []
                               | [_] -> true
                               | _ -> false
            forEach (List.indexed languageNames) <| fun (idx, langName) ->
                li [] [
                    button [yield on.click <| fun _ -> if (idx > 0)
                                                       then langName::removeIdx idx languageNames
                                                            |> SetLanguageOrder
                                                            |> dispatch
                                                       else ()
                            yield attr.``class`` "button"
                            if idx = 0 then yield attr.disabled "disabled"]
                           [text "To top"]
                    text langName
                    button [yield on.click <| fun _ -> match removeIdx idx languageNames with
                                                       | [] -> printfn "Can't remove last language"
                                                       | lessLanguages -> 
                                                            SetLanguageOrder lessLanguages
                                                            |> dispatch
                            yield attr.``class`` "button"
                            if mayNotRemove then yield attr.disabled "disabled"]
                           [text "Remove"]]
        | { configuration = None } -> Empty
    Main.General()
        .ConfigurationProjectName(
            configurationProjectName model,
            SetProjectName >> dispatch)
        .Languages(languages)
        .Elt()

let dataPage model (username: string) dispatch =
    Main.Data()
        .Reload(fun _ -> dispatch GetConfigurationMetadatas)
        .Username(username)
        .SignOut(fun _ -> dispatch SendSignOut)
        .Rows(cond model.configurationMetadatas <| function
            | None ->
                Main.EmptyData().Elt()
            | Some data ->
                forEach data <| fun conf ->
                    tr [] [
                        td []
                           [a [on.click <| fun _ -> CreateNewConfiguration conf |> dispatch]
                              [text conf.name]]
                        td [] [textf "%d parameters" conf.languageIndependent.Length]
                        td [] [textf "%d parameters" conf.languageDependent.Length]
                    ])
        .Elt()

let signInPage model dispatch =
    Main.SignIn()
        .Username(model.username, fun s -> dispatch (SetUsername s))
        .Password(model.password, fun s -> dispatch (SetPassword s))
        .SignIn(fun _ -> dispatch SendSignIn)
        .ErrorNotification(
            cond model.signInFailed <| function
            | false -> empty
            | true ->
                Main.ErrorNotification()
                    .HideClass("is-hidden")
                    .Text("Sign in failed. Use any username and the password \"password\".")
                    .Elt()
        )
        .Elt()

let menuItem (model: Model) (page: Page) (text: string) =
    Main.MenuItem()
        .Active(if model.page = page then "is-active" else "")
        .Url(router.Link page)
        .Text(text)
        .Elt()

let view model dispatch =
    Main()
        .Menu(concat [
            menuItem model Home "Home"
            menuItem model Data "New configuration"
            cond model.configuration <| function
            | None -> empty
            | Some x ->
                forEach
                    [(General, x.configurationProjectName)
                     (Common, "Common parameters")]
                    <| fun (page, menuName) ->
                        menuItem model page menuName
        ])
        .Body(
            cond model.page <| function
            | Home -> homePage model dispatch
            | General -> generalPage model dispatch
            | Common -> commonPage model dispatch
            | Data ->
                cond model.signedInAs <| function
                | Some username -> dataPage model username dispatch
                | None -> signInPage model dispatch
        )
        .Error(
            cond model.error <| function
            | None -> empty
            | Some err ->
                Main.ErrorNotification()
                    .Text(err)
                    .Hide(fun _ -> dispatch ClearError)
                    .Elt()
        )
        .Elt()

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        let configurationMetadataService = this.Remote<ConfigurationMetadataService>()
        let update = update configurationMetadataService this.JSRuntime
        Program.mkProgram (fun _ -> initModel, Cmd.ofMsg GetSignedInAs) update view
        |> Program.withRouter router
#if DEBUG
        |> Program.withHotReload
#endif
