module Client.Components.Navbar

open State
open Fable.React
open Fable.React.Props
open Fulma
open Feliz

type private NavbarState = {
    NavbarMenuActive: bool
} with
    static member init = {
        NavbarMenuActive = false
    }

type private NavbarMsg =
| Update_NavbarMenuActive of bool

let private update (state:NavbarState) (msg:NavbarMsg)=
    match msg with
    | Update_NavbarMenuActive (next) -> { state with NavbarMenuActive = next}

[<ReactComponent>]
let Navbar (model : Model) (dispatch : Msg -> unit) =
    let (state, dispatch) = React.useReducer(update, NavbarState.init)

    //let currentDisp = model.InformationSectionDisplay
    Navbar.navbar [Navbar.IsFixedTop; Navbar.CustomClass "is-dark csbNav"; Navbar.Props [Props.Role "navigation"; AriaLabel "main navigation" ]] [
        Navbar.Brand.div [] [
            Navbar.Item.a [Navbar.Item.Props [Props.Href "https://csb.bio.uni-kl.de/"]] [
                img [Props.Src "../Images/Logo.png"]
            ]
            Navbar.burger   [
                Navbar.Burger.IsActive state.NavbarMenuActive
                Navbar.Burger.Props [
                    Role "button"
                    AriaLabel "menu"
                    AriaExpanded false
                    OnClick (fun _ -> Update_NavbarMenuActive (not state.NavbarMenuActive) |> dispatch)
                ]
            ] [
                span [AriaHidden true] []
                span [AriaHidden true] []
                span [AriaHidden true] []
            ]
        ]
        Navbar.menu [Navbar.Menu.Props [Id "navbarMenu"; Class (if state.NavbarMenuActive then "navbar-menu is-active" else "navbar-menu") ]] [
            Navbar.Start.div [] [
                Navbar.Item.a
                    [
                        //Navbar.Item.Props [OnClick (fun _ -> ChangeHelpDisplay (if currentDisp = HowToUse then NoHelp else HowToUse) |> dispatch)]
                        //Navbar.Item.IsActive (currentDisp = HowToUse)
                    ] [
                    str "How to use"
                ]
                Navbar.Item.a
                    [
                        //Navbar.Item.Props [OnClick (fun _ -> ChangeHelpDisplay (if currentDisp = InputFormat then NoHelp else InputFormat) |> dispatch)]
                        //Navbar.Item.IsActive (currentDisp = InputFormat)
                    ] [
                    str "Input format"

                ]
                Navbar.Item.a
                    [
                        //Navbar.Item.Props [OnClick (fun _ -> ChangeHelpDisplay (if currentDisp = TechnicalScientificDetails then NoHelp else TechnicalScientificDetails) |> dispatch)]
                        //Navbar.Item.IsActive (currentDisp = TechnicalScientificDetails)
                    ] [
                    str "Technical details"
                ]
            ]
            Navbar.End.div [] [
                Navbar.Item.a
                    [
                        //Navbar.Item.Props [OnClick (fun _ -> ChangeHelpDisplay (if currentDisp = Contact then NoHelp else Contact) |> dispatch)]
                        //Navbar.Item.IsActive (currentDisp = Contact)
                    ] [
                    str "Contact"
                ]
            ]
        ]
    ]