module bolerogator.Server.Index

open Bolero
open Bolero.Html
open Bolero.Server.Html
open bolerogator

let page = doctypeHtml [] [
    head [] [
        meta [attr.charset "UTF-8"]
        meta [attr.name "viewport"; attr.content "width=device-width, initial-scale=1.0"]
        title [] [text "Bolero Application"]
        ``base`` [attr.href "/"]
        link [attr.rel "stylesheet"; attr.href "https://cdn.jsdelivr.net/npm/bulma@0.9.3/css/bulma.min.css"]
        link [attr.rel "stylesheet"; attr.href "css/index.css"]
        script [] [
            // Not the most practical way: it would be better to <script src="external-file.js"></script>,
            // but this is shorter.
            text "window.MyJsLib = window.MyJsLib || { focusById: function(id) { console.log(`focusById ${id}`); document.getElementById(id)?.focus(); } };"
        ]
    ]
    body [] [
        nav [attr.classes ["navbar"; "is-dark"]; "role" => "navigation"; attr.aria "label" "main navigation"] [
            div [attr.classes ["navbar-brand"]] [
                a [attr.classes ["navbar-item"; "has-text-weight-bold"; "is-size-5"]; attr.href "https://fsbolero.io"] [
                    img [attr.style "height:40px"; attr.src "https://github.com/fsbolero/website/raw/master/src/Website/img/wasm-fsharp.png"]
                    text "  Bolero"
                ]
            ]
        ]
        div [attr.id "main"] [rootComp<Client.Main.MyApp>]
        boleroScript
    ]
]
