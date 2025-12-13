import { Fetch_tryDelete_178CF8, Fetch_tryPost_178CF8, Fetch_tryPut_178CF8, Fetch_tryFetchAs_Z7E3650E0 } from "./fable_modules/Thoth.Fetch.2.0.0/Fetch.fs.js";
import { uncurry2 } from "./fable_modules/fable-library-js.4.13.0/Util.js";
import { decodeCatalogItemDTO, decodePrice, decodeCatalogItem, decodeCatalogItems } from "./Decoders.fs.js";
import { int, option } from "./fable_modules/Thoth.Json.5.0.0/Decode.fs.js";
import { decimal, object } from "./fable_modules/Thoth.Json.5.0.0/Encode.fs.js";
import { some } from "./fable_modules/fable-library-js.4.13.0/Option.js";
import { encodeCatalogItem } from "./Encoders.fs.js";

export function getCatalogItems() {
    return Fetch_tryFetchAs_Z7E3650E0("/api/items", uncurry2(decodeCatalogItems));
}

export function getCatalogItemById(id) {
    return Fetch_tryFetchAs_Z7E3650E0("/api/items/" + id, (path, value) => option(uncurry2(decodeCatalogItem), path, value));
}

export function getCatalogItemByDescription(description) {
    return Fetch_tryFetchAs_Z7E3650E0("/api/items/withdescription/" + description, uncurry2(decodeCatalogItems));
}

export function getCatalogItemByTypeBrand(catalogType, brand) {
    return Fetch_tryFetchAs_Z7E3650E0((("/api/items/type/" + catalogType) + "/brand/") + brand, uncurry2(decodeCatalogItems));
}

export function getCatalogItemByType(catalogType) {
    return Fetch_tryFetchAs_Z7E3650E0("/api/brand/all/type/" + catalogType, uncurry2(decodeCatalogItems));
}

export function getCatalogItemByBrand(brand) {
    return Fetch_tryFetchAs_Z7E3650E0("/api/types/all/brand/" + brand, uncurry2(decodeCatalogItems));
}

export function setPrice(id, price) {
    return Fetch_tryPut_178CF8("/api/item/price/" + id, some(object([["Price", decimal(price)]])), void 0, void 0, void 0, void 0, uncurry2(decodePrice));
}

export function setPicture(id, fileName, url) {
    return Fetch_tryPut_178CF8("/api/item/picture/" + id, some(object([["FileName", fileName], ["Uri", url]])), void 0, void 0, void 0, void 0, uncurry2(int));
}

export function setStock(id, stock) {
    return Fetch_tryPut_178CF8("/api/item/stock/" + id, some(object([["Stock", decimal(stock)]])), void 0, void 0, void 0, void 0, uncurry2(int));
}

export function setStockThreshold(id, reStock, maxStock) {
    return Fetch_tryPut_178CF8("/api/item/stockthreshold/" + id, some(object([["ReStock", decimal(reStock)], ["MaxStock", decimal(maxStock)]])), void 0, void 0, void 0, void 0, uncurry2(int));
}

export function setReorder(id, reorder) {
    return Fetch_tryPut_178CF8("/api/item/reorder/" + id, some(object([["Reoder", reorder]])), void 0, void 0, void 0, void 0, uncurry2(int));
}

export function createCatalogItem(catalogItem) {
    return Fetch_tryPost_178CF8("/api/item/new", some(encodeCatalogItem(catalogItem)), void 0, void 0, void 0, void 0, uncurry2(decodeCatalogItemDTO));
}

export function deleteCatalogItem(id) {
    return Fetch_tryDelete_178CF8("/api/item/" + id, void 0, void 0, void 0, void 0, void 0, uncurry2(int));
}

