namespace Belin.Cli

open Microsoft.VisualStudio.TestTools.UnitTesting
open System.Text.RegularExpressions

/// Contains assertions for working with unit tests.
[<RequireQualifiedAccess>]
module internal Should =

  /// Tests whether the specified values are equal.
  let inline be (expected: 'T) (actual: 'T) =
    Assert.AreEqual<'T>(expected, actual)

  /// Tests whether the specified object is an instance of the expected type.
  let inline beAssignableTo<'T> (value: objnull) =
    Assert.IsInstanceOfType<'T> value

  /// Tests whether the specified sequence is empty.
  let inline beEmpty (sequence: 'T seq) =
    Assert.IsEmpty sequence

  /// Tests whether the specified condition is false.
  let inline beFalse (condition: bool) =
    Assert.IsFalse condition

  /// Tests whether the specified object is `null`.
  let inline beNull (value: objnull) =
    Assert.IsNull value

  /// Tests whether the specified object is exactly an instance of the expected type.
  let inline beOfType<'T> (value: objnull) =
    Assert.IsExactInstanceOfType<'T> value

  /// Tests whether the specified value is contained in the given sequence.
  let inline beOneOf (sequence: 'T seq) (value: objnull) =
    Assert.Contains(value, sequence)

  /// Tests whether two sequences contain equal elements in the same order.
  let inline beSequence (expected: 'T seq) (actual: 'T seq) =
    Assert.AreSequenceEqual(expected, actual)

  /// Tests whether the specified condition is true.
  let inline beTrue (condition: bool) =
    Assert.IsTrue condition

  /// Tests whether the specified string ends with the given suffix.
  let inline endWith (expectedSuffix: string) (value: string) =
    Assert.EndsWith(expectedSuffix, value)

  /// Tests whether the specified string matches the given regular expression.
  let inline matchRegex (pattern: Regex) (value: string) =
    Assert.MatchesRegex(pattern, value)

  /// Tests whether the specified values are unequal.
  let inline notBe (notExpected: 'T) (actual: 'T) =
    Assert.AreNotEqual<'T>(notExpected, actual)

  /// Tests whether the specified object is not `null`.
  let inline notBeNull (value: objnull) =
    Assert.IsNotNull value

  /// Tests whether the specified string does not match the given regular expression.
  let inline notMatchRegex (pattern: Regex) (value: string) =
    Assert.DoesNotMatchRegex(pattern, value)

  /// Tests whether the specified string begins with the given prefix.
  let inline startWith (expectedPrefix: string) (value: string) =
    Assert.StartsWith(expectedPrefix, value)
