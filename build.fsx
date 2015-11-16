﻿#I @"packages/FAKE/tools"
#r "FakeLib.dll"

open Fake
open Fake.FileHelper
open System.IO

// ---------------------------------------------------------------------------- Variables

let buildSolutionFile = "./Json.Net.Unity3D.sln"
let buildConfiguration = "Release"
let binDir = "bin"
let testDir = binDir @@ "test"

// ------------------------------------------------------------------------- Unity Helper

let UnityPath = 
    @"C:\Program Files\Unity\Editor\Unity.exe" 

let Unity projectPath args = 
    let result = Shell.Exec(UnityPath, "-quit -batchmode -logFile -projectPath \"" + projectPath + "\" " + args) 
    if result < 0 then failwithf "Unity exited with error %d" result 

// ------------------------------------------------------------------------------ Targets

Target "Clean" (fun _ -> 
    CleanDirs [binDir]
)

Target "Build" (fun _ ->
    !! buildSolutionFile
    |> MSBuild "" "Rebuild" [ "Configuration", buildConfiguration ]
    |> Log "Build-Output: "
)

Target "Test" (fun _ ->  
    ensureDirectory testDir
    !! ("./src/**/bin/" + buildConfiguration + "/*.Tests.dll")
    |> NUnit (fun p -> 
        {p with 
            DisableShadowCopy = true;
            OutputFile = testDir @@ "TestResult.xml" })
)

Target "Package" (fun _ ->  
    Unity (Path.GetFullPath "src/UnityPackage") "-executeMethod PackageBuilder.BuildPackage"
    (!! "src/UnityPackage/*.unitypackage") |> Seq.iter (fun p -> MoveFile binDir p)
)

Target "Help" (fun _ ->  
    List.iter printfn [
      "usage:"
      "build [target]"
      ""
      " Targets for building:"
      " * Build        Build"
      " * Test         Test"
      " * Package      Build Unity Package"
      ""]
)

// --------------------------------------------------------------------------- Dependency

// Build order
"Clean"
  ==> "Build"
  ==> "Test"
  ==> "Package"

RunTargetOrDefault "Package"
