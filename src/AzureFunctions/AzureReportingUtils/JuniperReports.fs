module JuniperReports

open System
open Juniper.Ids
open Juniper.HeatPrognose
open Juniper
open Expecto
let reportInfo = 
    { ReportName = "Test"
      ReportTime = "Test"
      ReportIntervall = Monthly
      ReportTyp = "Test"
      ReportID = ReportId 1 }
let testLocation =
    [|  { Name = "Test1"
          LocationId = LocationId 1
          PostalCode = None }|]
let locationValues = 
    [| 
        { Value = 0.
          Description = "Measure 1"
          UnitOfMeasure = "MWh"
          Time = DateTime.Now };
        { Value = 1.
          Description = "Measure 2"
          UnitOfMeasure = "t"
          Time = DateTime.Now.AddYears(-1) }  |]

let sheetData =
    { Location = testLocation
      Measures = locationValues }      
let testSheetInsert = 
    let excelPackage = startExcelApp ()
    { ReportInformation = reportInfo
      ReportData = Some sheetData
      ExcelPackage = Some excelPackage }
let testWorkSheets = 
    printfn "testWorksheet"
    [ ReportSheet.testSheet, "TestWorksheet" ]

let expectoTests (reportData:ReportData) =
    let sheetInsert = 
        match reportData.SheetInsert with
        | Some sheetInsert -> sheetInsert
        | None -> failwith "no test possible"
    let sumMeasures = 
        match sheetInsert.ReportData with
        | Some data -> data.Measures |> Array.sumBy (fun x -> x.Value)
        | None -> 0.
         
    testList "Test if Sum of measures is not out of scope"
       [ testCase "Test Sum of measures is bigger or equal 0."
            <| fun () -> Expect.isGreaterThanOrEqual sumMeasures 0. "SumData should be bigger than or equal"]

open Microsoft.Azure.WebJobs
open FSharp.Control.Tasks.ContextInsensitive
open Microsoft.Extensions.Logging
open TriggerNames

[<FunctionName("JuniperReports")>]

let Run([<QueueTrigger(JuniperReports)>] content:string, log:ILogger) =
    task {
      let testSheetInsert = Newtonsoft.Json.JsonConvert.DeserializeObject<SheetInsert> content
      do!
            report {
              sheetInsert testSheetInsert
              testReportData expectoTests
              worksheetList testWorkSheets
              logSuccess "Finished testReport"
          }
      log.LogInformation ("Create TestReport")
     }