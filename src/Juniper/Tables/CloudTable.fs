module CloudTable

open Juniper.CreateTable
open Juniper.Logging
open TableNames
let weather = getTable Azure Weather connected
let location = getTable Azure Location connected