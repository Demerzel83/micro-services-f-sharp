namespace CatalogUI

[<RequireQualifiedAccess>]
/// Contains utility functions to work with value of the type `Deferred<'T>`.
module Deferred =

  type Deferred<'t> =
    | HasNotStartedYet
    | InProgress
    | Resolved of 't

  /// Returns whether the `Deferred<'T>` value has been resolved or not.
  let resolved = function
      | HasNotStartedYet -> false
      | InProgress -> false
      | Resolved _ -> true

  /// Returns whether the `Deferred<'T>` value is in progress or not.
  let inProgress = function
      | HasNotStartedYet -> false
      | InProgress -> true
      | Resolved _ -> false

  let exists (fn:'t -> bool) value =
    match value with
    | HasNotStartedYet -> HasNotStartedYet
    | InProgress -> InProgress
    | Resolved t -> Resolved (fn t)

  let map fn value =
    match value with
    | HasNotStartedYet -> HasNotStartedYet
    | InProgress -> InProgress
    | Resolved t -> Resolved (fn t)

  let bind fn value =
    match value with
    | HasNotStartedYet -> HasNotStartedYet
    | InProgress -> InProgress
    | Resolved v -> fn v

  let apply fn value =
    match (fn,value) with
    | (_, HasNotStartedYet) | (HasNotStartedYet, _) -> HasNotStartedYet
    | (InProgress, _) | (_, InProgress) -> InProgress
    | Resolved f, Resolved v -> Resolved (f v)