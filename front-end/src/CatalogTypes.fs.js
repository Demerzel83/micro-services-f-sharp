import { Record } from "./fable_modules/fable-library-js.4.13.0/Types.js";
import { record_type, string_type, class_type } from "./fable_modules/fable-library-js.4.13.0/Reflection.js";

export class CatalogType extends Record {
    constructor(Id, Type) {
        super();
        this.Id = Id;
        this.Type = Type;
    }
}

export function CatalogType_$reflection() {
    return record_type("CatalogUI.CatalogTypes.CatalogType", [], CatalogType, () => [["Id", class_type("System.Guid")], ["Type", string_type]]);
}

export class CatalogBrand extends Record {
    constructor(Id, Brand) {
        super();
        this.Id = Id;
        this.Brand = Brand;
    }
}

export function CatalogBrand_$reflection() {
    return record_type("CatalogUI.CatalogTypes.CatalogBrand", [], CatalogBrand, () => [["Id", class_type("System.Guid")], ["Brand", string_type]]);
}

export class CatalogItem extends Record {
    constructor(Name, Description, Type, Brand) {
        super();
        this.Name = Name;
        this.Description = Description;
        this.Type = Type;
        this.Brand = Brand;
    }
}

export function CatalogItem_$reflection() {
    return record_type("CatalogUI.CatalogTypes.CatalogItem", [], CatalogItem, () => [["Name", string_type], ["Description", string_type], ["Type", CatalogType_$reflection()], ["Brand", CatalogBrand_$reflection()]]);
}

export class CatalogItemDTO extends Record {
    constructor(Name, Description, Type, Brand) {
        super();
        this.Name = Name;
        this.Description = Description;
        this.Type = Type;
        this.Brand = Brand;
    }
}

export function CatalogItemDTO_$reflection() {
    return record_type("CatalogUI.CatalogTypes.CatalogItemDTO", [], CatalogItemDTO, () => [["Name", string_type], ["Description", string_type], ["Type", class_type("System.Guid")], ["Brand", class_type("System.Guid")]]);
}

