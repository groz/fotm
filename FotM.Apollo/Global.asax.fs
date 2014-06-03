namespace FotM.Apollo

open System
open System.Net.Http
open System.Web
open System.Web.Http
open System.Web.Mvc
open System.Web.Routing
open System.Web.Optimization
open System.Net.Http.Headers
open System.Threading.Tasks

type BundleConfig() =
    static member RegisterBundles (bundles:BundleCollection) =
        bundles.Add(ScriptBundle("~/bundles/jquery").Include([|"~/Scripts/jquery-{version}.js"|]))

        bundles.Add(ScriptBundle("~/bundles/angularjs")
            .Include(
                    [|
                        "~/Scripts/angular-1.2.16.js"
                        "~/Scripts/angular-route-1.2.16.js"
                        "~/Scripts/angular-cookies-1.2.16.js"
                    |]))


        // Use the development version of Modernizr to develop with and learn from. Then, when you're
        // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
        bundles.Add(ScriptBundle("~/bundles/modernizr").Include([|"~/Scripts/modernizr-*"|]))

        bundles.Add(ScriptBundle("~/bundles/bootstrap").Include(
                        "~/Scripts/bootstrap.js",
                        "~/Scripts/respond.js"))

        bundles.Add(StyleBundle("~/Content/css").Include(
                        "~/Content/Themes/Slate/bootstrap.css",
                        "~/Content/site.css"))

        bundles.Add(ScriptBundle("~/bundles/app")
            .Include(
                    [|
                    "~/app/app.js"
                    "~/app/appDebug.js"
                    "~/app/services/*.js"
                    "~/app/controllers/*.js"
                    "~/app/appConfig.js"
                    |])
            )

/// Route for ASP.NET MVC applications
type Route = { 
    controller : string
    action : string
    id : UrlParameter }

type HttpRoute = {
    controller : string
    id : RouteParameter }

type Global() =
    inherit System.Web.HttpApplication() 

    static member RegisterWebApi(config: HttpConfiguration) =
        // Configure routing
        config.MapHttpAttributeRoutes()
        config.Routes.MapHttpRoute(
            "DefaultApi", // Route name
            "api/{controller}/{id}", // URL with parameters
            { controller = "{controller}"; id = RouteParameter.Optional } // Parameter defaults
        ) |> ignore

        // Additional Web API settings
        config.Formatters.Insert(0, JsonNetFormatter())
        config.Formatters.XmlFormatter.UseXmlSerializer <- true

        config.EnableSystemDiagnosticsTracing() |> ignore

    static member RegisterFilters(filters: GlobalFilterCollection) =
        filters.Add(new HandleErrorAttribute())

    static member RegisterRoutes(routes:RouteCollection) =
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}")
        routes
            .MapRoute(
            "Debug", // Route name
            "debug/{action}", // catch all to route calls to SPA AngularJS router
            { controller = "Debug"; action = "Index"; id = UrlParameter.Optional } // Parameter defaults
            ) |> ignore
        routes
            .MapRoute(
            "Default", // Route name
            "{*catchAll}", // catch all to route calls to SPA AngularJS router
            { controller = "Home"; action = "Index"; id = UrlParameter.Optional } // Parameter defaults
        ) |> ignore

    member x.Application_Start() =
        AreaRegistration.RegisterAllAreas()

        GlobalConfiguration.Configure(Action<_> Global.RegisterWebApi)
        Global.RegisterFilters(GlobalFilters.Filters)
        Global.RegisterRoutes(RouteTable.Routes)
        BundleConfig.RegisterBundles BundleTable.Bundles
        
        Main.OnStart()

open global.Owin

type Startup() =
    member this.Configuration(app: IAppBuilder): unit =        
        app.MapSignalR() |> ignore

