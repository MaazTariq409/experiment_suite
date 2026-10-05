# Experiment suite evidence map (for revision response)

This file maps reviewer concerns to concrete artifacts in `experiment_suite/`.

| Reviewer concern | Artifact that answers it |
|---|---|
| Why are tasks enterprise-oriented? | `docs/01_ENTERPRISE_ORIENTATION.md` + layered Contracts/Participant structure |
| Standalone vs existing app? | Participant implements inside supplied baseline; not a blank console app |
| What did participants see? | `Contracts/` + `Participant/` + `task_specs/`; Evaluation excluded by `scripts/pack_participant.ps1` |
| How do tests run on independent solutions? | `docs/02_EVALUATION_MODEL.md` — tests bind only to frozen interfaces/DTOs |
| Why complete if tests fail? | `docs/03_COMPLETION_CRITERIA.md` — submit-or-time-limit stopping rule |
| Eight task pairs only listed by category | `task_specs/` + per-variant Contracts/Participant/Evaluation |
| Static-analysis tool/version/config | `analyzer/` + `docs/05_STATIC_ANALYSIS.md` (SonarAnalyzer.CSharp 10.35.0.4138; Metrics 5.6.0) |
| A/B equivalence | Matched capability + mirrored validation/error contracts; domain nouns differ |
| Reproducibility / data availability | `data/` Excel+CSV package + this public GitHub repository |

## Recommended public release layout

1. Publish this `experiment_suite` (with Evaluation) to a private OSF/Zenodo/GitHub repo with reviewer access.
2. Keep `reference_solutions/PRIVATE_DO_NOT_DISTRIBUTE` out of the participant channel.
3. Replace bracketed analyzer versions in `docs/05_STATIC_ANALYSIS.md` with the real frozen toolchain before camera-ready.

## Current completeness

| Area | Status |
|---|---|
| Shared enterprise primitives | Complete |
| TP01 Contracts/Participant/Evaluation + detailed specs | Complete (10 hidden tests each; reference verified) |
| TP02 Contracts/Participant/Evaluation + detailed specs | Complete (20 hidden tests each; reference verified) |
| TP03 Contracts/Participant/Evaluation + detailed specs | Complete (20 hidden tests each; reference verified) |
| TP04 Contracts/Participant/Evaluation + detailed specs | Complete (20 hidden tests each; reference verified) |
| TP05 Contracts/Participant/Evaluation + detailed specs | Complete (20 hidden tests each; reference verified) |
| TP06 Contracts/Participant/Evaluation + detailed specs | Complete (20 hidden tests each; reference verified) |
| TP07 Contracts/Participant/Evaluation + detailed specs | Complete (20 hidden tests each; legacy starter + reference verified) |
| TP08 Contracts/Participant/Evaluation + detailed specs | Complete (20 hidden tests each; reference verified) |
| Hidden Evaluation harness | **All TP01–TP08 complete** |
| Reference implementations | TP01–TP08 A/B |
| Static analyzer freeze | Complete — SonarAnalyzer.CSharp **10.35.0.4138** + Metrics **5.6.0**; hash `c84cde8e4805433f` |
| Suite-aligned dataset (Excel + CSV) | Complete — `data/AI_NET_Experimental_Dataset_SuiteAligned.xlsx` |

## Important integrity note

If the original data-collection materials differed from this reconstructed suite, document that explicitly in the response letter (e.g., “materials reconstructed to match Protocol v2.0 frozen task architecture”) and do not claim byte-identical identity with unavailable originals. Prefer aligning the revision narrative to Protocol v2.0 + this suite.
