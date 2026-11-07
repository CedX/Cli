namespace Belin.Cli

open Microsoft.VisualStudio.TestTools.UnitTesting
open System
open System.Text.RegularExpressions

/// Contains assertions for working with unit tests.
[<RequireQualifiedAccess>]
module internal Should =

  /// Tests whether the specified values are equal.
  let be (expected: 'T) (actual: 'T) =
    Assert.AreEqual<'T>(expected, actual)

  /// Tests whether the specified object is an instance of the expected type.
  let beAssignableTo (expectedType: Type) (value: 'T) =
    Assert.IsInstanceOfType(value, expectedType)

  /// Tests whether the specified sequence is empty.
  let beEmpty (sequence: 'T seq) =
    Assert.IsEmpty sequence

  /// Tests whether the specified condition is false.
  let beFalse (condition: bool) =
    Assert.IsFalse condition

  /// Tests whether the specified object is `null`.
  let beNull (value: 'T) =
    Assert.IsNull value

  /// Tests whether the specified object is exactly an instance of the expected type.
  let beOfType (expectedType: Type) (value: 'T) =
    Assert.IsExactInstanceOfType(value, expectedType)

  /// Tests whether the specified value is contained in the given sequence.
  let beOneOf (sequence: 'T seq) (value: 'T) =
    Assert.Contains<'T>(value, sequence)

  /// Tests whether two sequences contain equal elements in the same order.
  let beSequence (expected: 'T seq) (actual: 'T seq) =
    Assert.AreSequenceEqual(expected, actual)

  /// Tests whether the specified condition is true.
  let beTrue (condition: bool) =
    Assert.IsTrue condition

  /// Tests whether the specified string ends with the given suffix.
  let endWith (expectedSuffix: string) (value: string) =
    Assert.EndsWith(expectedSuffix, value)

  /// Tests whether the specified string matches the given regular expression.
  let matchRegex (pattern: Regex) (value: string) =
    Assert.MatchesRegex(pattern, value)

  /// Tests whether the specified values are unequal.
  let notBe (notExpected: 'T) (actual: 'T) =
    Assert.AreNotEqual<'T>(notExpected, actual)

  /// Tests whether the specified object is not `null`.
  let notBeNull (value: 'T) =
    Assert.IsNotNull value

  /// Tests whether the specified string does not match the given regular expression.
  let notMatchRegex (pattern: Regex) (value: string) =
    Assert.DoesNotMatchRegex(pattern, value)

  /// Tests whether the specified action does not throw an exception of the expected type.
  let notThrow (exceptionType: Type) (action: unit -> unit) =
    try
      action()
      Assert.IsTrue true
    with ex ->
      Assert.IsNotInstanceOfType(ex, exceptionType)

  /// Tests whether the specified string begins with the given prefix.
  let startWith (expectedPrefix: string) (value: string) =
    Assert.StartsWith(expectedPrefix, value)

  /// Tests whether the specified action throws an exception of the expected type.
  let throw (exceptionType: Type) (action: unit -> unit) =
    try
      action()
      Assert.Fail "The specified action did not throw any exception."
    with ex ->
      Assert.IsInstanceOfType(ex, exceptionType)

/// Contains asynchronous assertions for working with unit tests.
[<RequireQualifiedAccess>]
module internal ShouldAsync =

  /// Tests whether the specified values are equal.
  let be (expected: 'T) (actual: Async<'T>) = task {
    let! actual = actual
    return actual |> Should.be expected
  }

  /// Tests whether the specified object is an instance of the expected type.
  let beAssignableTo (expectedType: Type) (value: Async<'T>) = task {
    let! value = value
    return value |> Should.beAssignableTo expectedType
  }

  /// Tests whether the specified sequence is empty.
  let beEmpty (sequence: Async<'T seq>) = task {
    let! sequence = sequence
    return Should.beEmpty sequence
  }

  /// Tests whether the specified condition is false.
  let beFalse (condition: Async<bool>) = task {
    let! condition = condition
    return Should.beFalse condition
  }

  /// Tests whether the specified object is `null`.
  let beNull (value: Async<'T>) = task {
    let! value = value
    return Should.beNull value
  }

  /// Tests whether the specified object is exactly an instance of the expected type.
  let beOfType (expectedType: Type) (value: Async<'T>) = task {
    let! value = value
    return value |> Should.beOfType expectedType
  }

  /// Tests whether the specified value is contained in the given sequence.
  let beOneOf (sequence: 'T seq) (value: Async<'T>) = task {
    let! value = value
    return value |> Should.beOneOf sequence
  }

  /// Tests whether two sequences contain equal elements in the same order.
  let beSequence (expected: 'T seq) (actual: Async<'T seq>) = task {
    let! actual = actual
    return actual |> Should.beSequence expected
  }

  /// Tests whether the specified condition is true.
  let beTrue (condition: Async<bool>) = task {
    let! condition = condition
    return Should.beTrue condition
  }

  /// Tests whether the specified string ends with the given suffix.
  let endWith (expectedSuffix: string) (value: Async<string>) = task {
    let! value = value
    return value |> Should.endWith expectedSuffix
  }

  /// Tests whether the specified string matches the given regular expression.
  let matchRegex (pattern: Regex) (value: Async<string>) = task {
    let! value = value
    return value |> Should.matchRegex pattern
  }

  /// Tests whether the specified values are unequal.
  let notBe (notExpected: 'T) (actual: Async<'T>) = task {
    let! actual = actual
    return actual |> Should.notBe notExpected
  }

  /// Tests whether the specified object is not `null`.
  let notBeNull (value: Async<'T>) = task {
    let! value = value
    return Should.notBeNull value
  }

  /// Tests whether the specified string does not match the given regular expression.
  let notMatchRegex (pattern: Regex) (value: Async<string>) = task {
    let! value = value
    return value |> Should.notMatchRegex pattern
  }

  /// Tests whether the specified string begins with the given prefix.
  let startWith (expectedPrefix: string) (value: Async<string>) = task {
    let! value = value
    return value |> Should.startWith expectedPrefix
  }
