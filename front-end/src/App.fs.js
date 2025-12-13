import { Deferred$1 } from "./deferred.fs.js";
import { Msg, AsyncOperationStatus$1, State } from "./AppTypes.fs.js";
import { Cmd_OfFunc_result } from "./fable_modules/Fable.Elmish.3.0.0/./cmd.fs.js";
import { ProgramModule_mkProgram, ProgramModule_run } from "./fable_modules/Fable.Elmish.3.0.0/program.fs.js";
import { Program_withReactSynchronous } from "./fable_modules/Fable.Elmish.React.3.0.1/react.fs.js";
import { update } from "./Update.fs.js";
import { render } from "./Render.fs.js";

export function init() {
    return [new State(new Deferred$1(0, []), new Deferred$1(0, []), false), Cmd_OfFunc_result(new Msg(1, [new AsyncOperationStatus$1(0, [])]))];
}

ProgramModule_run(Program_withReactSynchronous("elmish-app", ProgramModule_mkProgram(init, update, render)));

