# Completion criteria and stopping rule

## Operational rule (frozen)

A task observation ends when **either**:

1. the participant **declares completion and submits** the Participant project for evaluation; **or**
2. the participant reaches the **predefined maximum task time**.

The timer does **not** wait for all hidden tests to pass.

## Why this matters

Completion time and test failures are intentionally separable outcomes:
- a fast submission may still have failing tests;
- a slow submission may pass all tests.

This preserves the productivity outcome as an implementation-time measure and the correctness outcome as a post-hoc automated measure.

## What “task complete” does **not** mean
- It does not mean “all tests passed”.
- It does not mean “AI confirmed the solution”.
- It does not mean “researcher manually accepted the code”.

## Recommended session reporting fields
- `task_start`, `task_end`
- `stop_reason` ∈ {`participant_submit`, `time_limit`}
- `build_succeeded` (bool)
- `test_failures` (int)
- `tests_total` (int)
