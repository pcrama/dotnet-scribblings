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
    | [<EndPoint "/counter">] Counter
    | [<EndPoint "/data">] Data

/// The Elmish application's model.
type Model =
    {
        page: Page
        counter: int
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
        counter = 0
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
    | Increment
    | Decrement
    | SetCounter of int
    | GetConfigurationMetadatas
    | GotConfigurationMetadatas of ConfigurationMetadata[]
    | CreateNewConfiguration of ConfigurationMetadata
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
         traverse tryCreateLanguageDependent conf.languageDependent
         |> Result.map
             (fun languageDependents ->
              {
                  configurationProjectName = name
                  Languages = ["English"]
                  Parameters = languageIndependents
                  LanguageDependent = languageDependents
              }))

let update remote jsRuntime message model =
    let onSignIn = function
        | Some _ -> Cmd.ofMsg GetConfigurationMetadatas
        | None -> Cmd.none
    match message with
    | SetPage page ->
        { model with page = page }, Cmd.none

    | Increment ->
        { model with counter = model.counter + 1 }, Cmd.none
    | Decrement ->
        { model with counter = model.counter - 1 }, Cmd.none
    | SetCounter value ->
        { model with counter = value }, Cmd.OfJS.either jsRuntime "MyJsLib.focusById" [| value % 2 |> sprintf "p%d" |] (fun _ -> ClearError) Error

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

let homePage model _dispatch =
    Main.Home()
        .Title(match model.configuration with
               | Some { configurationProjectName = configurationProjectName } -> configurationProjectName
               | None -> "No Project yet.")
        .Elt()

let counterPage model dispatch =
    Main.Counter()
        .Decrement(fun _ -> dispatch Decrement)
        .Increment(fun _ -> dispatch Increment)
        .Value(model.counter, fun v -> dispatch (SetCounter v))
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
            | Some x -> menuItem model Counter x.configurationProjectName
        ])
        .Body(
            cond model.page <| function
            | Home -> homePage model dispatch
            | Counter -> counterPage model dispatch
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
