import { Union, Record } from "./fable_modules/fable-library-js.4.13.0/Types.js";
import { CatalogItemDTO_$reflection, CatalogItem_$reflection } from "./CatalogTypes.fs.js";
import { decimal_type, class_type, union_type, record_type, bool_type, option_type, list_type } from "./fable_modules/fable-library-js.4.13.0/Reflection.js";
import { Deferred$1_$reflection } from "./deferred.fs.js";
import { FetchError_$reflection } from "./fable_modules/Thoth.Fetch.2.0.0/Fetch.fs.js";
import { FSharpResult$2 } from "./fable_modules/fable-library-js.4.13.0/Result.js";

export class State extends Record {
    constructor(CatalogItems, CatalogItem, Loading) {
        super();
        this.CatalogItems = CatalogItems;
        this.CatalogItem = CatalogItem;
        this.Loading = Loading;
    }
}

export function State_$reflection() {
    return record_type("CatalogUI.AppTypes.State", [], State, () => [["CatalogItems", Deferred$1_$reflection(list_type(CatalogItem_$reflection()))], ["CatalogItem", Deferred$1_$reflection(option_type(CatalogItem_$reflection()))], ["Loading", bool_type]]);
}

export class AsyncOperationStatus$1 extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Started", "Finished"];
    }
}

export function AsyncOperationStatus$1_$reflection(gen0) {
    return union_type("CatalogUI.AppTypes.AsyncOperationStatus`1", [gen0], AsyncOperationStatus$1, () => [[], [["Item", gen0]]]);
}

export class Msg extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["GetCatalogItem", "GetCatalogItems", "SaveCatalogItem", "SetPrice"];
    }
}

export function Msg_$reflection() {
    return union_type("CatalogUI.AppTypes.Msg", [], Msg, () => [[["Item1", class_type("System.Guid")], ["Item2", AsyncOperationStatus$1_$reflection(union_type("Microsoft.FSharp.Core.FSharpResult`2", [option_type(CatalogItem_$reflection()), FetchError_$reflection()], FSharpResult$2, () => [[["ResultValue", option_type(CatalogItem_$reflection())]], [["ErrorValue", FetchError_$reflection()]]]))]], [["Item", AsyncOperationStatus$1_$reflection(union_type("Microsoft.FSharp.Core.FSharpResult`2", [list_type(CatalogItem_$reflection()), FetchError_$reflection()], FSharpResult$2, () => [[["ResultValue", list_type(CatalogItem_$reflection())]], [["ErrorValue", FetchError_$reflection()]]]))]], [["Item1", CatalogItemDTO_$reflection()], ["Item2", AsyncOperationStatus$1_$reflection(union_type("Microsoft.FSharp.Core.FSharpResult`2", [CatalogItemDTO_$reflection(), FetchError_$reflection()], FSharpResult$2, () => [[["ResultValue", CatalogItemDTO_$reflection()]], [["ErrorValue", FetchError_$reflection()]]]))]], [["Item1", class_type("System.Guid")], ["Item2", decimal_type], ["Item3", AsyncOperationStatus$1_$reflection(union_type("Microsoft.FSharp.Core.FSharpResult`2", [decimal_type, FetchError_$reflection()], FSharpResult$2, () => [[["ResultValue", decimal_type]], [["ErrorValue", FetchError_$reflection()]]]))]]]);
}

