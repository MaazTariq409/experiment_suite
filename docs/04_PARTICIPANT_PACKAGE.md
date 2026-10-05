# Participant package contents

For each assigned variant, the participant zip contains:

```
TP0xY/
  Contracts/                 (read-only)
  Participant/               (implementation area)
  TASK_SPEC.md               (requirements & acceptance criteria)
  README_PARTICIPANT.txt
```

Plus once per session:
```
Enterprise.Shared/
ENVIRONMENT.md               (.NET SDK, IDE, permitted tooling)
```

Explicitly excluded from participant zips:
- `Evaluation/`
- `reference_solutions/`
- answer keys / scoring rubrics beyond the task spec

## AI-condition note

Because hidden tests are absent from the workspace, the AI assistant can read only participant-visible artifacts (specs, contracts, stubs), not the confirmatory oracle.
