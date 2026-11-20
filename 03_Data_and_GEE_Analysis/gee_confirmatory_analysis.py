"""
Confirmatory Gaussian GEE analysis for COLA-D-26-00246.

Model (as reported in the manuscript / SAP Appendix E.5):
    Outcome ~ Condition + Period + TaskPair + Condition:Period + Condition:Order

Coding
------
Traditional is the reference level for Condition, so the reported coefficient is
the AI-minus-Traditional contrast (negative = faster / fewer defects under AI for
lower-is-better outcomes).

Working correlation: exchangeable
Clustering: participant_id

Aliased-term handling
---------------------
In this counterbalanced crossover, Sequence, Period, Condition, and Order are
linearly dependent for saturated Condition×Order specifications. Including
Condition×Order with Condition + Period makes the design matrix rank-deficient
(the same structural issue that produced singular mixed-effects fits).

This script therefore:
1. Builds the confirmatory design exactly as specified in the SAP/manuscript;
2. Reports the numeric rank of the design matrix;
3. Drops linearly dependent (aliased) columns using a QR-based pivot rule
   before calling statsmodels GEE, and logs which terms were removed;
4. Fits GEE on the reduced full-rank design and exports the main-effect
   Condition[AI] coefficient (interactions are never reported as the AI effect).

Empirically, the Condition×Order columns are the aliased terms that are dropped;
the estimable confirmatory model that reproduces Table 6 is therefore
Condition + Period + TaskPair + Condition×Period.

Primary inference in the paper remains the paired participant×task-pair contrast.
GEE is confirmatory / design-adjusted.
"""

from __future__ import annotations

import json
from decimal import Decimal, ROUND_HALF_UP
from pathlib import Path

import numpy as np
import pandas as pd
import statsmodels.api as sm
from patsy import dmatrices


def round_half_up(value: float, places: int = 3) -> Decimal:
    quant = Decimal("1").scaleb(-places)
    return Decimal(str(value)).quantize(quant, rounding=ROUND_HALF_UP)


ROOT = Path(__file__).resolve().parent
DATA = ROOT / "observations_master.csv"
OUT_DIR = ROOT / "gee_outputs"
OUT_DIR.mkdir(exist_ok=True)

OUTCOMES = [
    ("completion_time_min", "CompletionTime"),
    ("test_failures", "TestFailures"),
    ("code_smells", "CodeSmells"),
    ("cyclomatic_complexity", "CyclomaticComplexity"),
    ("maintainability_index", "MaintainabilityIndex"),
    ("loc", "LOC"),
    ("nasa_tlx", "NASATLX"),
]

# Manuscript Table 6 targets (for self-check; not used in estimation).
TABLE6_TARGETS = {
    "completion_time_min": -5.283,
    "test_failures": -1.033,
    "code_smells": -1.371,
    "cyclomatic_complexity": -0.842,
    "maintainability_index": 5.788,
    "loc": 14.096,
    "nasa_tlx": -10.167,
}


def prepare(df: pd.DataFrame) -> pd.DataFrame:
    df = df.copy()

    # Order: Traditional-first vs AI-first, derived from sequence allocation.
    # Sequences A/C start with Traditional; B/D start with AI.
    df["order"] = np.where(df["sequence"].isin(["A", "C"]), "TraditionalFirst", "AIFirst")

    # Explicit category orders so Traditional is the Condition reference under
    # Treatment coding (alphabetical default would make AI the reference).
    df["condition"] = pd.Categorical(df["condition"], categories=["Traditional", "AI"])
    df["period"] = pd.Categorical(df["period"].astype(int).astype(str), categories=["1", "2"])
    df["task_pair"] = pd.Categorical(df["task_pair"])
    df["order"] = pd.Categorical(df["order"], categories=["TraditionalFirst", "AIFirst"])
    df["sequence"] = pd.Categorical(df["sequence"])
    return df


def drop_aliased_columns(exog: pd.DataFrame, tol: float = 1e-10) -> tuple[pd.DataFrame, list[str]]:
    """Remove linearly dependent columns (aliased terms) via QR with column pivoting."""
    x = exog.to_numpy(dtype=float)
    try:
        import scipy.linalg as sla

        _, r, piv = sla.qr(x, mode="economic", pivoting=True)
        diag = np.abs(np.diag(r))
        threshold = tol * (diag.max() if diag.size else 1.0)
        rank = int(np.sum(diag > threshold))
        keep_idx = sorted(piv[:rank].tolist())
    except Exception:
        u, s, vt = np.linalg.svd(x, full_matrices=False)
        threshold = tol * (s.max() if s.size else 1.0)
        rank = int(np.sum(s > threshold))
        scores = np.sum(np.abs(vt[:rank, :]) ** 2, axis=0)
        keep_idx = sorted(np.argsort(scores)[-rank:].tolist())

    kept = exog.iloc[:, keep_idx]
    dropped = [c for i, c in enumerate(exog.columns) if i not in set(keep_idx)]
    return kept, dropped


def pick_ai_main_effect(params: pd.Series) -> str:
    """Select the main-effect Condition[AI] term; never an interaction."""
    main = [
        k
        for k in params.index
        if "condition" in k.lower() and "AI" in k and ":" not in k
    ]
    if not main:
        raise KeyError(
            "No main-effect Condition[AI] term found in: " + ", ".join(map(str, params.index))
        )
    return main[0]


def fit_gee(df: pd.DataFrame, outcome: str) -> dict:
    # Explicit Treatment('Traditional') so the AI coefficient is AI − Traditional.
    formula = (
        f"{outcome} ~ C(condition, Treatment('Traditional')) + C(period) + C(task_pair) "
        f"+ C(condition, Treatment('Traditional')):C(period) "
        f"+ C(condition, Treatment('Traditional')):C(order)"
    )

    y, X = dmatrices(formula, df, return_type="dataframe")
    rank_before = int(np.linalg.matrix_rank(X.to_numpy(dtype=float)))
    X_reduced, dropped = drop_aliased_columns(X)
    rank_after = int(np.linalg.matrix_rank(X_reduced.to_numpy(dtype=float)))

    model = sm.GEE(
        endog=y,
        exog=X_reduced,
        groups=df.loc[y.index, "participant_id"],
        cov_struct=sm.cov_struct.Exchangeable(),
        family=sm.families.Gaussian(),
    )
    result = model.fit()

    ai_key = pick_ai_main_effect(result.params)
    ci = result.conf_int().loc[ai_key]

    # Because Period 1 is the reference period, ai_key is the Period 1
    # AI-minus-Traditional contrast. The Period 2 contrast is the sum of
    # the Condition main effect and the Condition x Period 2 interaction.
    interaction_candidates = [
        k for k in result.params.index
        if "condition" in k.lower()
        and "AI" in k
        and ":C(period)[T.2]" in k
    ]
    if len(interaction_candidates) != 1:
        raise KeyError(
            "Expected one AI-by-Period-2 interaction term; found: "
            + ", ".join(interaction_candidates)
        )
    interaction_key = interaction_candidates[0]
    contrast = np.zeros(len(result.params), dtype=float)
    contrast[list(result.params.index).index(ai_key)] = 1.0
    contrast[list(result.params.index).index(interaction_key)] = 1.0
    period2_test = result.t_test(contrast)
    period2_ci = np.asarray(period2_test.conf_int()).reshape(-1)
    period2_effect = float(np.asarray(period2_test.effect).reshape(-1)[0])

    return {
        "outcome": outcome,
        "n_obs": int(len(y)),
        "n_groups": int(df.loc[y.index, "participant_id"].nunique()),
        "formula": formula,
        "rank_before": rank_before,
        "rank_after": rank_after,
        "aliased_columns_dropped": dropped,
        "ai_term_period1": ai_key,
        "period1_ai_coef": float(result.params[ai_key]),
        "period1_ai_se": float(result.bse[ai_key]),
        "period1_ai_pvalue": float(result.pvalues[ai_key]),
        "period1_ai_ci_low": float(ci[0]),
        "period1_ai_ci_high": float(ci[1]),
        "condition_by_period2_term": interaction_key,
        "condition_by_period2_coef": float(result.params[interaction_key]),
        "condition_by_period2_pvalue": float(result.pvalues[interaction_key]),
        "period2_ai_contrast": period2_effect,
        "period2_ai_pvalue": float(np.asarray(period2_test.pvalue).reshape(-1)[0]),
        "period2_ai_ci_low": float(period2_ci[0]),
        "period2_ai_ci_high": float(period2_ci[1]),
        "converged": bool(getattr(result, "converged", True)),
        "table6_target": TABLE6_TARGETS.get(outcome),
        "matches_table6_3dp": (
            round_half_up(float(result.params[ai_key]))
            == round_half_up(TABLE6_TARGETS[outcome])
            if outcome in TABLE6_TARGETS
            else None
        ),
    }


def main() -> None:
    df = pd.read_csv(DATA)
    df = prepare(df)

    alias_log = {
        "policy": (
            "If the confirmatory design matrix is rank-deficient, linearly dependent "
            "columns are dropped after QR pivoting. Dropped columns are logged as aliased. "
            "Condition uses Treatment coding with Traditional as the reference level. "
            "Only the main-effect Condition[AI] coefficient is reported as the adjusted "
            "AI effect (interaction terms are retained in the model when estimable but "
            "are not reported as the AI effect). Sequence is not entered as a separate "
            "main effect because Order is derived from Sequence "
            "(A/C = Traditional-first; B/D = AI-first)."
        ),
        "note": (
            "For this design, Condition×Order columns are typically aliased and dropped; "
            "the estimable confirmatory specification that reproduces manuscript Table 6 "
            "is Condition + Period + TaskPair + Condition×Period."
        ),
    }

    rows = []
    for col, label in OUTCOMES:
        info = fit_gee(df, col)
        info["label"] = label
        rows.append(info)
        print(
            f"{label}: Period1 AI coef={info['period1_ai_coef']:.4f} "
            f"[{info['period1_ai_ci_low']:.3f}, {info['period1_ai_ci_high']:.3f}], "
            f"P1-p={info['period1_ai_pvalue']:.4g}, "
            f"Period2 contrast={info['period2_ai_contrast']:.4f}, "
            f"matches_Table6={info['matches_table6_3dp']}, "
            f"dropped={info['aliased_columns_dropped']}"
        )

    # Holm adjustment across the seven Period 1 GEE condition contrasts.
    from statsmodels.stats.multitest import multipletests

    period1_p = [r["period1_ai_pvalue"] for r in rows]
    period1_holm = multipletests(period1_p, method="holm")[1]
    for row, adjusted in zip(rows, period1_holm):
        row["period1_ai_holm_pvalue"] = float(adjusted)

    out_json = OUT_DIR / "gee_confirmatory_results.json"
    out_csv = OUT_DIR / "gee_confirmatory_results.csv"
    out_alias = OUT_DIR / "aliased_term_handling.json"

    with out_json.open("w", encoding="utf-8") as f:
        json.dump(rows, f, indent=2)
    pd.DataFrame(rows).to_csv(out_csv, index=False)
    with out_alias.open("w", encoding="utf-8") as f:
        json.dump(alias_log, f, indent=2)

    print(f"Wrote {out_json}")
    print(f"Wrote {out_csv}")
    print(f"Wrote {out_alias}")

    if not all(r["matches_table6_3dp"] for r in rows):
        raise SystemExit("GEE coefficients do not match manuscript Table 6 to 3 d.p.")


if __name__ == "__main__":
    main()
