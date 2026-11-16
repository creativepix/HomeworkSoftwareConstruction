---
name: csprog
description: "Implement and verify code in C#."
version: 0.1.0
author: Vladislav Sidelnikov
license: MIT
platforms: [windows]
metadata:
  hermes:
    tags: [KPO, CSharp, Homework, Testing]
    related_skills: [test-driven-development, requesting-code-review]
---

# C# programmer

Implement the limited stages in a "student-code" style using C#, and then verify their actual behavior in practice-do not just stop at compilation.

## Procedure

1. Read the stage prompt, README, tests, interfaces, current implementation, style profile and course examples.
2. Never use git or change `.git`.
3. Append `AI_REPORT.md` before coding. Preserve the original prompt and identify your AI role.
4. Make minimal changes touching architecture. Use ordinary C#, explicit flow, and course-level features. Do not add any patterns or packages. If it is necessary, consult with me.
5. Keep boundaries: `Program.cs` starts the program; console handles input/output; application services coordinate; domain owns business rules; tests check and validate the correctness of code and validate expected bad input and save domain invariants.
6. If new logic is made, add behavioral tests for success, rejection, and boundaries. Test concrete key services directly; for orchestration use mock.
7. Sequentially execute the relevant `dotnet build` and `dotnet test` commands, perform code coverage analysis, and run end-to-end smoke tests. Do not weaken the tests themselves if the error is perceived - instead of it, address the root causes of problems.
8. Finish/append `AI_REPORT.md` with files, behavior, exact commands and results, follow-ups.

## Homework1 remarks/problems

- Mixing of responsibilities in `Program.cs`. The logic should be moved to a separate class.
- An interface should also be defined for `Park`, and the code should interact with that interface.
- The program crashes if an empty inventory number is entered.
- One of the classes (`ServiceCenter`) is not covered by tests at all.

## Hidden problems

- Catching all exceptions hides defects.
- Do not falsify results or mark unperformed checks as passed.
- Do not hide AI implementation in reports.

## Verification

List modified files, exact checks and outcomes, coverage if measured, smoke scenarios. `AI_REPORT.md` must contain the same facts.
