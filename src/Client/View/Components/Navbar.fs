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

let private update (state:NavbarState) (msg:NavbarMsg) =
    match msg with
    | Update_NavbarMenuActive (next) -> { state with NavbarMenuActive = next}

[<ReactComponent>]
let Navbar (model : Model) (dispatch : Msg -> unit) =
    let (state, dispatch_inner) = React.useReducer(update, NavbarState.init)

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
                    OnClick (fun _ -> Update_NavbarMenuActive (not state.NavbarMenuActive) |> dispatch_inner)
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
                        Navbar.Item.Props [OnClick (fun _ -> UpdatePage Page.Main |> dispatch)]
                        Navbar.Item.IsActive (model.Page = Page.Main)
                    ] [
                    str "Predict"
                ]
                Navbar.Item.a
                    [
                        Navbar.Item.Props [OnClick (fun _ -> UpdatePage Page.About |> dispatch)]
                        Navbar.Item.IsActive (model.Page = Page.About)
                    ] [
                    str "About"
                ]
                Navbar.Item.a
                    [
                        Navbar.Item.Props [OnClick (fun _ -> UpdatePage Page.PrivacyPolicy |> dispatch)]
                        Navbar.Item.IsActive (model.Page = Page.PrivacyPolicy)
                    ] [
                    str "Privacy Policy"
                ]
                Navbar.Item.a
                    [
                        Navbar.Item.Props [OnClick (fun _ -> UpdatePage Page.Contact |> dispatch)]
                        Navbar.Item.IsActive (model.Page = Page.Contact)
                    ] [
                    str "Contact"
                ]
            ]
            Navbar.End.div [] [
                Navbar.Item.a
                    [
                        Navbar.Item.Props [Href "https://github.com/CSBiology/deepStabP"]
                        //Navbar.Item.IsActive (currentDisp = Contact)
                    ] [
                    Icon.icon [
                        Icon.Size IsLarge
                    ] [
                        Html.i [prop.className "fas fa-2x fa-brands fa-github "]
                    ]
                ]
            ]
        ]
    ]