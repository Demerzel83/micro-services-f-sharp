import { Cmd_OfPromise_perform, Cmd_none } from "./fable_modules/Fable.Elmish.3.0.0/cmd.fs.js";
import { Deferred$1 } from "./deferred.fs.js";
import { Msg, AsyncOperationStatus$1, State } from "./AppTypes.fs.js";
import { setPrice, createCatalogItem, getCatalogItems, getCatalogItemById } from "./Api.fs.js";
import { Cmd_OfFunc_result } from "./fable_modules/Fable.Elmish.3.0.0/./cmd.fs.js";
import { equals } from "./fable_modules/fable-library-js.4.13.0/Util.js";

export function update(msg, state) {
    let matchResult, id, id_2, result, items, catalogItemDto, catalogItem_1, id_3, id_4, price, id_5, price_2, r_3;
    switch (msg.tag) {
        case 1: {
            if (msg.fields[0].tag === 1) {
                if (msg.fields[0].fields[0].tag === 0) {
                    matchResult = 5;
                    items = msg.fields[0].fields[0].fields[0];
                }
                else {
                    matchResult = 10;
                }
            }
            else if (equals(state.CatalogItems, new Deferred$1(1, []))) {
                matchResult = 3;
            }
            else {
                matchResult = 4;
            }
            break;
        }
        case 2: {
            if (msg.fields[1].tag === 1) {
                if (msg.fields[1].fields[0].tag === 0) {
                    matchResult = 7;
                    catalogItem_1 = msg.fields[0];
                    id_3 = msg.fields[1].fields[0].fields[0];
                }
                else {
                    matchResult = 10;
                }
            }
            else {
                matchResult = 6;
                catalogItemDto = msg.fields[0];
            }
            break;
        }
        case 3: {
            if (msg.fields[2].tag === 1) {
                if (msg.fields[2].fields[0].tag === 0) {
                    matchResult = 9;
                    id_5 = msg.fields[0];
                    price_2 = msg.fields[1];
                    r_3 = msg.fields[2].fields[0].fields[0];
                }
                else {
                    matchResult = 10;
                }
            }
            else {
                matchResult = 8;
                id_4 = msg.fields[0];
                price = msg.fields[1];
            }
            break;
        }
        default:
            if (msg.fields[1].tag === 1) {
                if (msg.fields[1].fields[0].tag === 0) {
                    matchResult = 2;
                    id_2 = msg.fields[0];
                    result = msg.fields[1].fields[0].fields[0];
                }
                else {
                    matchResult = 10;
                }
            }
            else if (equals(state.CatalogItem, new Deferred$1(1, []))) {
                matchResult = 0;
            }
            else {
                matchResult = 1;
                id = msg.fields[0];
            }
    }
    switch (matchResult) {
        case 0:
            return [state, Cmd_none()];
        case 1:
            return [new State(state.CatalogItems, new Deferred$1(1, []), state.Loading), Cmd_OfPromise_perform(getCatalogItemById, id, (r) => (new Msg(0, [id, new AsyncOperationStatus$1(1, [r])])))];
        case 2:
            return [new State(state.CatalogItems, new Deferred$1(2, [result]), state.Loading), Cmd_none()];
        case 3:
            return [state, Cmd_none()];
        case 4:
            return [new State(new Deferred$1(1, []), state.CatalogItem, state.Loading), Cmd_OfPromise_perform(getCatalogItems, void 0, (arg) => (new Msg(1, [new AsyncOperationStatus$1(1, [arg])])))];
        case 5:
            return [new State(new Deferred$1(2, [items]), state.CatalogItem, state.Loading), Cmd_none()];
        case 6:
            return [new State(state.CatalogItems, state.CatalogItem, true), Cmd_OfPromise_perform(createCatalogItem, catalogItemDto, (r_1) => (new Msg(2, [catalogItemDto, new AsyncOperationStatus$1(1, [r_1])])))];
        case 7:
            return [new State(state.CatalogItems, state.CatalogItem, false), Cmd_OfFunc_result(new Msg(1, [new AsyncOperationStatus$1(0, [])]))];
        case 8:
            return [new State(state.CatalogItems, state.CatalogItem, true), Cmd_OfPromise_perform((price_1) => setPrice(id_4, price_1), price, (r_2) => (new Msg(3, [id_4, price, new AsyncOperationStatus$1(1, [r_2])])))];
        case 9:
            return [new State(state.CatalogItems, state.CatalogItem, false), Cmd_OfFunc_result(new Msg(1, [new AsyncOperationStatus$1(0, [])]))];
        default:
            throw new Error("Match failure: CatalogUI.AppTypes.Msg");
    }
}

