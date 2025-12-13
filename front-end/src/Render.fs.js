import { CatalogItemDTO } from "./CatalogTypes.fs.js";
import { singleton, ofArray, map } from "./fable_modules/fable-library-js.4.13.0/List.js";
import { mkAttr, reactApi, reactElement } from "./fable_modules/Feliz.0.68.10/./Interop.fs.js";
import { map as map_1, defaultArg } from "./fable_modules/fable-library-js.4.13.0/Option.js";
import { createObj } from "./fable_modules/fable-library-js.4.13.0/Util.js";
import { Msg, AsyncOperationStatus$1 } from "./AppTypes.fs.js";
import { fromParts } from "./fable_modules/fable-library-js.4.13.0/Decimal.js";

export function getRandomCatalogItem() {
    return new CatalogItemDTO("CFE", "Created from the front-end", "1de512a8-d377-4869-a792-b46bb5125fee", "b1a357ec-f9fb-42df-aa56-5c2712b84c79");
}

export function catalogItemRows(catalogItems) {
    return map((catalogItem) => reactElement("div", {
        children: reactApi.Children.toArray([catalogItem.Name, catalogItem.Description, catalogItem.Type.Type, catalogItem.Brand.Brand]),
    }), catalogItems);
}

export function showCatalogItems(catalogItems) {
    let children_1;
    const children_2 = ofArray([reactElement("div", {
        children: reactApi.Children.toArray(["Full List"]),
    }), (children_1 = catalogItemRows(catalogItems), reactElement("div", {
        children: reactApi.Children.toArray(children_1),
    }))]);
    return reactElement("div", {
        children: reactApi.Children.toArray(children_2),
    });
}

export function showCatalogItem(catalogItem) {
    return defaultArg(map_1((catalogItem_1) => {
        let children_3;
        const children_4 = ofArray([reactElement("div", {
            children: reactApi.Children.toArray(["Catlog item by id"]),
        }), (children_3 = ofArray([reactElement("h3", {
            children: reactApi.Children.toArray(["Description"]),
        }), reactElement("div", {
            children: reactApi.Children.toArray([catalogItem_1.Name]),
        })]), reactElement("div", {
            children: reactApi.Children.toArray(children_3),
        }))]);
        return reactElement("div", {
            children: reactApi.Children.toArray(children_4),
        });
    }, catalogItem), "No Item");
}

export function actions(dispatch) {
    let children_1;
    const children_2 = ofArray([reactElement("div", {
        children: reactApi.Children.toArray(["Action"]),
    }), (children_1 = ofArray([reactElement("button", createObj(ofArray([mkAttr("onClick", (_arg) => {
        dispatch(new Msg(0, ["7ecf255b-0942-49c8-b053-f3ba5b67c58e", new AsyncOperationStatus$1(0, [])]));
    }), mkAttr("children", "Fetch Catalog Item By Id")]))), reactElement("button", createObj(ofArray([mkAttr("onClick", (_arg_1) => {
        dispatch(new Msg(2, [getRandomCatalogItem(), new AsyncOperationStatus$1(0, [])]));
    }), mkAttr("children", "Create new catalog item")]))), reactElement("button", createObj(ofArray([mkAttr("onClick", (_arg_2) => {
        dispatch(new Msg(3, ["7ecf255b-0942-49c8-b053-f3ba5b67c58e", fromParts(666, 0, 0, false, 0), new AsyncOperationStatus$1(0, [])]));
    }), mkAttr("children", "Set Price")])))]), reactElement("div", {
        children: reactApi.Children.toArray(children_1),
    }))]);
    return reactElement("div", {
        children: reactApi.Children.toArray(children_2),
    });
}

export function render(state, dispatch) {
    let matchValue, matchValue_1, matchValue_2, children, children_1, children_2, children_3;
    const children_4 = ofArray([(matchValue = state.CatalogItem, (matchValue_1 = state.CatalogItems, (matchValue_2 = state.Loading, (matchValue.tag === 0) ? ((matchValue_1.tag === 1) ? ((children = singleton(reactElement("label", createObj(singleton(mkAttr("children", "Loading ..."))))), reactElement("div", {
        children: reactApi.Children.toArray(children),
    }))) : ((matchValue_1.tag === 0) ? (matchValue_2 ? ((children = singleton(reactElement("label", createObj(singleton(mkAttr("children", "Loading ..."))))), reactElement("div", {
        children: reactApi.Children.toArray(children),
    }))) : ((children_1 = singleton(reactElement("label", createObj(singleton(mkAttr("children", "Nothing is loaded"))))), reactElement("div", {
        children: reactApi.Children.toArray(children_1),
    })))) : (matchValue_2 ? ((children = singleton(reactElement("label", createObj(singleton(mkAttr("children", "Loading ..."))))), reactElement("div", {
        children: reactApi.Children.toArray(children),
    }))) : ((children_1 = singleton(reactElement("label", createObj(singleton(mkAttr("children", "Nothing is loaded"))))), reactElement("div", {
        children: reactApi.Children.toArray(children_1),
    })))))) : ((matchValue.tag === 2) ? ((matchValue_1.tag === 0) ? (matchValue_2 ? ((children = singleton(reactElement("label", createObj(singleton(mkAttr("children", "Loading ..."))))), reactElement("div", {
        children: reactApi.Children.toArray(children),
    }))) : ((children_2 = singleton(reactElement("label", createObj(singleton(mkAttr("children", "Nothing is loaded"))))), reactElement("div", {
        children: reactApi.Children.toArray(children_2),
    })))) : ((matchValue_1.tag === 2) ? (matchValue_2 ? ((children = singleton(reactElement("label", createObj(singleton(mkAttr("children", "Loading ..."))))), reactElement("div", {
        children: reactApi.Children.toArray(children),
    }))) : ((children_3 = ofArray([showCatalogItems(matchValue_1.fields[0]), showCatalogItem(matchValue.fields[0])]), reactElement("div", {
        children: reactApi.Children.toArray(children_3),
    })))) : ((children = singleton(reactElement("label", createObj(singleton(mkAttr("children", "Loading ..."))))), reactElement("div", {
        children: reactApi.Children.toArray(children),
    }))))) : ((children = singleton(reactElement("label", createObj(singleton(mkAttr("children", "Loading ..."))))), reactElement("div", {
        children: reactApi.Children.toArray(children),
    }))))))), actions(dispatch)]);
    return reactElement("div", {
        children: reactApi.Children.toArray(children_4),
    });
}

