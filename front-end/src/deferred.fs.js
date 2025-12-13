import { Union } from "./fable_modules/fable-library-js.4.13.0/Types.js";
import { union_type } from "./fable_modules/fable-library-js.4.13.0/Reflection.js";

export class Deferred$1 extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["HasNotStartedYet", "InProgress", "Resolved"];
    }
}

export function Deferred$1_$reflection(gen0) {
    return union_type("CatalogUI.Deferred.Deferred`1", [gen0], Deferred$1, () => [[], [], [["Item", gen0]]]);
}

/**
 * Returns whether the `Deferred<'T>` value has been resolved or not.
 */
export function resolved(_arg) {
    switch (_arg.tag) {
        case 1:
            return false;
        case 2:
            return true;
        default:
            return false;
    }
}

/**
 * Returns whether the `Deferred<'T>` value is in progress or not.
 */
export function inProgress(_arg) {
    switch (_arg.tag) {
        case 1:
            return true;
        case 2:
            return false;
        default:
            return false;
    }
}

export function exists(fn, value) {
    switch (value.tag) {
        case 1:
            return new Deferred$1(1, []);
        case 2:
            return new Deferred$1(2, [fn(value.fields[0])]);
        default:
            return new Deferred$1(0, []);
    }
}

export function map(fn, value) {
    switch (value.tag) {
        case 1:
            return new Deferred$1(1, []);
        case 2:
            return new Deferred$1(2, [fn(value.fields[0])]);
        default:
            return new Deferred$1(0, []);
    }
}

export function bind(fn, value) {
    switch (value.tag) {
        case 1:
            return new Deferred$1(1, []);
        case 2:
            return fn(value.fields[0]);
        default:
            return new Deferred$1(0, []);
    }
}

export function apply(fn, value) {
    let matchResult, f, v;
    switch (value.tag) {
        case 1: {
            switch (fn.tag) {
                case 0: {
                    matchResult = 0;
                    break;
                }
                default:
                    matchResult = 1;
            }
            break;
        }
        case 2: {
            switch (fn.tag) {
                case 1: {
                    matchResult = 1;
                    break;
                }
                case 2: {
                    matchResult = 2;
                    f = fn.fields[0];
                    v = value.fields[0];
                    break;
                }
                default:
                    matchResult = 0;
            }
            break;
        }
        default:
            matchResult = 0;
    }
    switch (matchResult) {
        case 0:
            return new Deferred$1(0, []);
        case 1:
            return new Deferred$1(1, []);
        default:
            return new Deferred$1(2, [f(v)]);
    }
}

