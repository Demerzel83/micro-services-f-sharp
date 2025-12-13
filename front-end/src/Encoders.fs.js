import { guid, object } from "./fable_modules/Thoth.Json.5.0.0/Encode.fs.js";

export function encodeCatalogItem(catalogItem) {
    return object([["Name", catalogItem.Name], ["Description", catalogItem.Description], ["Type", guid(catalogItem.Type)], ["Brand", guid(catalogItem.Brand)]]);
}

