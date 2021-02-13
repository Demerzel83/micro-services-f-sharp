namespace Microsoft.eShopOnContainers.Services.Location.Core

open System
open MongoDB.Driver.GeoJsonObjectModel

module LocationTypes =
    type LocationPoint = {
        Type : string
        Longitude : double
        Latitude : double
    }

    type LocationPolygon = {
        Type : string
        coordinates : GeoJson2DGeographicCoordinates list list list
    }

    type Locations = {
        Id : string
        LocationId : string
        Code : string
        Parent_Id : string
        Description : string
        Latitude : double
        Longitude : double
        Location : LocationPoint
        Polygon : LocationPolygon
    }

    type UserLocation = {
        Id : string
        UserId : string
        LocationId : int
        UpdateDate : DateTime
    }

    type UserLocationDetails = {
        LocationId : int
        Code : string
        Description : string
    }