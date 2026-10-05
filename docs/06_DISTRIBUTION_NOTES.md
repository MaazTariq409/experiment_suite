# Restoring participant stubs (distribution mode)

TP01 A/B currently contain **reference implementations** so the evaluator can be demonstrated.

For actual participant packages:
1. Keep Evaluation out of the zip (`scripts/pack_participant.ps1`).
2. Replace TP01 service files with NotImplemented stubs (or regenerate via `scripts/generate_suite.py` and re-copy only Contracts/Participant stubs).
3. Leave TP02–TP08 as NotImplemented stubs (current default).

Do not distribute `reference_solutions/PRIVATE_DO_NOT_DISTRIBUTE`.
