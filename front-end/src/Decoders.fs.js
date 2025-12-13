import { list, decimal, string, guid, object } from "./fable_modules/Thoth.Json.5.0.0/Decode.fs.js";
import { CatalogItemDTO, CatalogItem, CatalogBrand, CatalogType } from "./CatalogTypes.fs.js";
import { uncurry2 } from "./fable_modules/fable-library-js.4.13.0/Util.js";

export const decodeCatalogType = (path_2) => ((v) => object((x) => {
    let objectArg, objectArg_1;
    return new CatalogType((objectArg = x.Required, objectArg.Field("id", guid)), (objectArg_1 = x.Required, objectArg_1.Field("type", string)));
}, path_2, v));

export const decodeCatalogBrand = (path_2) => ((v) => object((x) => {
    let objectArg, objectArg_1;
    return new CatalogBrand((objectArg = x.Required, objectArg.Field("id", guid)), (objectArg_1 = x.Required, objectArg_1.Field("brand", string)));
}, path_2, v));

export const decodeCatalogItem = (path_2) => ((v) => object((x) => {
    let objectArg, objectArg_1, objectArg_2, objectArg_3;
    return new CatalogItem((objectArg = x.Required, objectArg.Field("name", string)), (objectArg_1 = x.Required, objectArg_1.Field("description", string)), (objectArg_2 = x.Required, objectArg_2.Field("catalogType", uncurry2(decodeCatalogType))), (objectArg_3 = x.Required, objectArg_3.Field("catalogBrand", uncurry2(decodeCatalogBrand))));
}, path_2, v));

export const decodeCatalogItemDTO = (path_4) => ((v) => object((x) => {
    let objectArg, objectArg_1, objectArg_2, objectArg_3;
    return new CatalogItemDTO((objectArg = x.Required, objectArg.Field("name", string)), (objectArg_1 = x.Required, objectArg_1.Field("description", string)), (objectArg_2 = x.Required, objectArg_2.Field("type", guid)), (objectArg_3 = x.Required, objectArg_3.Field("brand", guid)));
}, path_4, v));

export const decodePrice = (path_1) => ((v) => object((x) => {
    const objectArg = x.Required;
    return objectArg.Field("price", decimal);
}, path_1, v));

export const decodeCatalogItems = (path) => ((value) => list(uncurry2(decodeCatalogItem), path, value));

