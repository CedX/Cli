namespace Belin.Cli

open System

/// An enumeration of instruction set architectures.
type Architecture =
  | X64 = 1
  | X86 = 2

/// COntains operations for working with architectures.
module Architecture =

  /// The current architecture.
  let Current = if Environment.Is64BitOperatingSystem then Architecture.X64 else Architecture.X86
