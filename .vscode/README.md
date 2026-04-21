# VS Code workspace configuration

This folder contains a minimal set of tasks and launch configurations to build,
test, and debug ChessV from VS Code on Windows. It assumes the
`ms-dotnettools.csharp` (or C# Dev Kit) extension is installed; nothing else is
required.

## Running tests

- **All tests**: `Ctrl+Shift+P` → **Tasks: Run Test Task** (this runs the
  default `test-all` task, equivalent to `dotnet test ChessV.sln --nologo`).
- **Just one project**: `Ctrl+Shift+P` → **Tasks: Run Task** → pick
  `test-chessv` or `test-apmw`.
- **Just the test class in the active editor**: `Tasks: Run Task` →
  `test-current-file`. This uses `${fileBasenameNoExtension}` as a
  `FullyQualifiedName~...` filter, so opening `CrashReportFormatTests.cs` and
  running the task runs only those tests.
- **Known failures bucket**: `Tasks: Run Task` → `test-known-failures` runs the
  `Repros_2026_02_12` class plus anything tagged `[TestCategory("KnownLimit")]`.

## Debugging tests

1. Set a breakpoint in the test (or production) code you want to inspect.
2. Open the **Run and Debug** view and pick **Debug: Current Test File** (or
   **Debug: All Tests**).
3. Press **F5**. The test host launches with `VSTEST_HOST_DEBUG=1`, which makes
   it print a line like `Process Id: 12345, Name: testhost` and **pause**
   waiting for a debugger.
4. Open the command palette → **Debug: Attach to .NET 5+ or .NET Core process**
   and select that PID. Execution resumes and your breakpoint will hit.

The two-step launch+attach dance is the standard pattern for debugging code
that runs under `dotnet test`; there is no single-click alternative that works
reliably across the C# extension and C# Dev Kit.

## Environment variables

- `CHESSV_DISABLE_MAKEUNMAKE_ASSERT=1` — disables the make/unmake round-trip
  assertion that ChessV runs in debug builds. The default is **on** (i.e. the
  assertion fires). Set this to `1` in your shell or in the `env` block of a
  launch config if you want to skip it locally while iterating on something
  unrelated.
