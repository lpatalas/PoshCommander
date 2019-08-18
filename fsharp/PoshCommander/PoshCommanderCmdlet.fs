namespace PoshCommander

open System
open System.Management.Automation

[<Cmdlet(VerbsLifecycle.Invoke, "PoshCommander")>]
type PoshCommanderCmdlet() =
    inherit PSCmdlet()
    
    [<Parameter(Position = 0)>]
    member val LeftPath = String.Empty with get, set

    [<Parameter(Position = 1)>]
    member val RightPath = String.Empty with get, set

    [<Parameter>]
    member val EditorPath = String.Empty with get, set

    [<Parameter>]
    member val ViewerPath = String.Empty with get, set

    override this.BeginProcessing() =
        this.Host.UI.WriteLine("F# cmdlet")

